using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Linq;
using System.Collections;

public class DRL_CatcherBot : Agent
{
    [Header("Agent Settings")]
    public bool debugStart = false;
    public float speed = 8f;
    public string targetTag = "PlayerCamper";
    public float visionRadius = 15f;
    public float targetSwitchInterval = 2f;
    public float pointProximity = 10f;
    public GameObject[] patrolPoints;

    private Rigidbody2D rbody;
    [SerializeField] private GameObject currentTarget;
    private int currentTargetHP;
    private int currentTargetMaxHP;

    [Header("Border Data")]
    public float minX = -24.4f;
    public float maxX = 17.5f;
    public float minY = -34f;
    public float maxY = 17f;

    private bool isRunning = false;
    private Vector3 lastPosition;

    private List<GameObject> targets = new();
    private bool useAdvancedDecision = false;

    private float targetSwitchTimer = 0f;
    private int currentPatrolIndex = 0;
    [SerializeField, ReadOnly] private bool isPatrolling = false;
    private Vector2 patrolTargetOffset = Vector2.zero;

    // State management
    private enum BotState { Passive, Aggressive }
    [SerializeField, ReadOnly] private BotState botState = BotState.Passive;
    [SerializeField, ReadOnly] private int soulsCollected = 0;
    public int soulsToAggressive = 20;
    private string soulTag = "Soul";
    [SerializeField] private GameObject soulPrefab;
    private List<GameObject> souls = new();
    private Coroutine createSoulCoroutine;


    private float aggressiveTimer = 0f;
    public float aggressiveDuration = 30f;

    private void Start()
    {
        if (debugStart)
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("PlayerCamper");
            StartInference(players.ToList(), useAdvancedDecision);
        }
    }

    public override void Initialize()
    {
        rbody = GetComponent<Rigidbody2D>();
        lastPosition = transform.localPosition;
    }

    public override void OnEpisodeBegin() { }

    public void StartInference(List<GameObject> players, bool useAdvanced = false)
    {
        targets = players;
        useAdvancedDecision = useAdvanced;
        isRunning = true;

        currentTarget = null;
        lastPosition = transform.localPosition;
        targetSwitchTimer = 0f;
        currentPatrolIndex = 0;

        CreateSoul(10);
        SetState(BotState.Passive);
    }

    private float Normalize(float val, float min, float max)
    {
        return (val - min) / (max - min);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (!isRunning)
        {
            sensor.AddObservation(Vector2.zero);
            sensor.AddObservation(Vector2.zero);
            sensor.AddObservation(0f);
            return;
        }

        float normAgentX = Normalize(transform.localPosition.x, minX, maxX);
        float normAgentY = Normalize(transform.localPosition.y, minY, maxY);
        sensor.AddObservation(normAgentX);
        sensor.AddObservation(normAgentY);

        if (currentTarget != null)
        {
            float normTargetX = Normalize(currentTarget.transform.localPosition.x, minX, maxX);
            float normTargetY = Normalize(currentTarget.transform.localPosition.y, minY, maxY);
            sensor.AddObservation(normTargetX);
            sensor.AddObservation(normTargetY);

            sensor.AddObservation(useAdvancedDecision ? currentTargetHP / (float)currentTargetMaxHP : 0f);
        }
        else
        {
            sensor.AddObservation(0f);
            sensor.AddObservation(0f);
            sensor.AddObservation(0f);
        }
    }

    private void SetState(BotState state)
    {
        botState = state;
        soulsCollected = 0;
        currentTarget = null;
        isPatrolling = false;
        aggressiveTimer = 0f;
        ActivateSouls(state == BotState.Passive);

        foreach (var target in targets)
        {
            target.SetActive(state == BotState.Aggressive);
        }

        if (state == BotState.Passive)
        {
            if (createSoulCoroutine == null)
            {
                createSoulCoroutine = StartCoroutine(CreateSoulCoroutine());
            }
            GameEvents.Instance.StopAttack();
        }
        else
        {
            if (createSoulCoroutine != null)
            {
                StopCoroutine(createSoulCoroutine);
                createSoulCoroutine = null;
            }

            isRunning = false;
            ProgressBar.Instance.StartProgress(3f, transform.position, false, () =>
            {
                isRunning = true;
            });

            GameEvents.Instance.StartAttack();
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (!isRunning)
            return;

        // State transition
        if (botState == BotState.Passive && soulsCollected >= soulsToAggressive)
        {
            SetState(BotState.Aggressive);
        }

        // State transition: Aggressive → Passive (через час)
        if (botState == BotState.Aggressive)
        {
            aggressiveTimer += Time.deltaTime;
            if (aggressiveTimer >= aggressiveDuration)
            {
                SetState(BotState.Passive);
            }
        }

        targetSwitchTimer += Time.deltaTime;
        if (targetSwitchTimer >= targetSwitchInterval)
        {
            UpdateTarget();
            UpdateSoulTarget();
            targetSwitchTimer = 0f;
        }

        /*
        float h = actions.ContinuousActions[0];
        float v = actions.ContinuousActions[1];

        Vector2 movement = new Vector2(h, v) * speed * Time.deltaTime;
        rbody.MovePosition(rbody.position + movement);*/

        Vector2 raw = new Vector2(
            actions.ContinuousActions[0],
            actions.ContinuousActions[1]
        );
        // нормалізуємо (якщо є рух)
        Vector2 dir = raw.sqrMagnitude > 1e-6f
            ? raw.normalized
            : Vector2.zero;

        // фіксуємо сталу швидкість
        Vector2 movement = dir * speed * Time.deltaTime;
        rbody.MovePosition(rbody.position + movement);

        

        //Debug.Log($"Action: h = {h}, v = {v}, movement = {movement}");

        // Target logic based on state
        if (botState == BotState.Passive)
        {
            // Якщо ціль не Soul або втрачена, шукаємо нову Soul
            if (currentTarget == null || !currentTarget.CompareTag(soulTag) || !IsTargetVisible(currentTarget))
            {
                UpdateSoulTarget();
            }

            // Якщо немає Soul поблизу — патрулюємо
            if (currentTarget == null)
            {
                Patrol();
            }
            else
            {
                isPatrolling = false;
            }
        }
        else // Aggressive
        {
            // Якщо ціль втрачена або не в полі зору, оновлюємо ціль
            if (currentTarget == null || !targets.Contains(currentTarget) || !IsTargetVisible(currentTarget))
            {
                UpdateTarget();
            }

            // Якщо немає видимих цілей — патрулюємо точки
            if (currentTarget == null)
            {
                Patrol();
            }
            else
            {
                isPatrolling = false;
            }
        }
    }

    // Оновлення цілі для агресивного стану (гравці)
    private void UpdateTarget()
    {
        if (botState != BotState.Aggressive)
            return;

        var visibleTargets = targets.Where(t => t != null && IsTargetVisible(t)).ToList();
        if (visibleTargets.Count > 0)
        {
            currentTarget = SelectTarget(visibleTargets);
            var targetCamper = currentTarget.GetComponent<DRL_TargetCamper>();
            if (targetCamper)
            {
                currentTargetHP = targetCamper.HP;
                currentTargetMaxHP = targetCamper.MaxHP;
            }
            else
            {
                currentTargetHP = 0;
                currentTargetMaxHP = 0;
            }
            isPatrolling = false;
        }
        else
        {
            currentTarget = null;
            isPatrolling = true;
        }
    }

    // Оновлення цілі для пасивного стану (душі)
    private void UpdateSoulTarget()
    {
        if (botState != BotState.Passive)
            return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, visionRadius);
        GameObject bestSoul = null;
        float bestDist = float.MaxValue;

        foreach (var hit in hits)
        {
            if (hit.CompareTag(soulTag))
            {
                float dist = Vector2.Distance(transform.localPosition, hit.transform.localPosition);
                if (dist < bestDist)
                {
                    bestSoul = hit.gameObject;
                    bestDist = dist;
                }
            }
        }

        if (bestSoul != null)
        {
            currentTarget = bestSoul;
            isPatrolling = false;
        }
        else
        {
            currentTarget = null;
            isPatrolling = true;
        }
    }

    private void CreateSoul(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (souls.Count >= 15) return;

            Vector3 randomPoint = new Vector3(Random.Range(-24.4f, 17f), Random.Range(-34f, 17f), 0);

            Debug.Log("ss");
            souls.Add(Instantiate(soulPrefab, randomPoint, Quaternion.identity));
        }
    }

    private void ActivateSouls(bool activate)
    {
        foreach (var soul in souls)
        {
            soul.gameObject.SetActive(activate);
        }
    }

    // Вибір цілі з видимих
    private GameObject SelectTarget(List<GameObject> candidates)
    {
        GameObject best = null;
        float bestScore = float.MaxValue;

        foreach (var t in candidates)
        {
            if (t == null) continue;

            float dist = Vector2.Distance(transform.localPosition, t.transform.localPosition);
            float score = dist;

            if (useAdvancedDecision)
            {
                var targetCamper = t.GetComponent<DRL_TargetCamper>();
                score -= targetCamper.HP / (float)targetCamper.MaxHP;
            }

            if (score < bestScore)
            {
                best = t;
                bestScore = score;
            }
        }

        return best;
    }

    private bool IsTargetVisible(GameObject target)
    {
        if (target == null) return false;
        float dist = Vector2.Distance(transform.localPosition, target.transform.localPosition);
        return dist <= visionRadius;
    }

    private void Patrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
            return;

        isPatrolling = true;

        GameObject patrolPoint = patrolPoints[currentPatrolIndex];

        // Якщо ще не задано patrolTargetOffset або ми досягли попередньої цілі, генеруємо нову випадкову точку в радіусі 5
        if (currentTarget != patrolPoint || patrolTargetOffset == Vector2.zero)
        {
            float offsetX = Random.Range(-1f, 1f);
            float offsetY = Random.Range(-1f, 1f);
            patrolTargetOffset = new Vector2(offsetX, offsetY);
            currentTarget = patrolPoint;
        }

        Vector2 patrolTargetPos = (Vector2)patrolPoint.transform.position + patrolTargetOffset;

        //Vector2 newPos = Vector2.MoveTowards(rbody.position, patrolTargetPos, speed * Time.deltaTime);
        //rbody.MovePosition(newPos);

        float dist = Vector2.Distance(transform.position, patrolTargetPos);
        if (dist < pointProximity)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            patrolTargetOffset = Vector2.zero;
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // Залишаємо порожнім, бо працюємо лише з inference
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (botState == BotState.Passive && collision.CompareTag(soulTag))
        {
            // Підбір душі
            soulsCollected++;
            souls.Remove(collision.gameObject);
            Destroy(collision.gameObject);
            currentTarget = null;
            return;
        }

        if (botState == BotState.Aggressive && collision.CompareTag(targetTag))
        {
            Catch(collision.gameObject);
        }
    }

    private void Catch(GameObject target)
    {
        target.transform.position = new Vector3(0, 100, 0);
        //targets.Remove(target);

        if (targets.Count == 0)
        {
            EndEpisode();
        }

        currentTarget = null;
    }

    private IEnumerator CreateSoulCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(3f);
            CreateSoul(1);
        }
    }
}
