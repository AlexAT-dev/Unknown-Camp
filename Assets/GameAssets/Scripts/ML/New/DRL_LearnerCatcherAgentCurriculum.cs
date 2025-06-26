using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class DRL_LearnerCatcherAgentCurriculum : Agent
{
    public bool debug = false;

    [Header("Agent Settings")]
    public float speed = 6f;
    private Rigidbody2D rbody;
    private DRL_TargetCamper currentTarget;
    private float lastDistanceToTarget;
    private float predictionFactor = 1f;

    [Header("Targets")]
    public string targetTag = "PlayerCamper";
    public DRL_TargetCamper targetPrefab;
    public List<DRL_TargetCamper> targets = new();
    public Transform environment;
    public SpriteRenderer ground;

    [Header("Border Data")]
    public float minX = -17.5f;
    public float maxX = 17.5f;
    public float minY = -10f;
    public float maxY = 10f;


    [Header("Curriculum Parameters")]
    public int targetCount = 1;
    public bool targetsAreMoving = false;
    public bool useAdvancedDecision = false;

    private float totalTargets;
    private int patienceCounter;
    private float rewardTimer;
    private Vector3 lastPosition;
    private Vector2 _previousDirection;
    private int _stepCount;
    public int maxEpisodeSteps = 1000;

    public override void Initialize()
    {
        rbody = GetComponent<Rigidbody2D>();

        var envParams = Academy.Instance.EnvironmentParameters;

        targetCount = Mathf.FloorToInt(envParams.GetWithDefault("target_count", targetCount));
        targetsAreMoving = envParams.GetWithDefault("targets_moving", targetsAreMoving ? 1f : 0f) > 0f;
        useAdvancedDecision = envParams.GetWithDefault("use_advanced", useAdvancedDecision ? 1f : 0f) > 0f;
    }

    public override void OnEpisodeBegin()
    {
        var envParams = Academy.Instance.EnvironmentParameters;
        targetCount = Mathf.FloorToInt(envParams.GetWithDefault("target_count", targetCount));
        targetsAreMoving = envParams.GetWithDefault("targets_moving", targetsAreMoving ? 1f : 0f) > 0f;
        useAdvancedDecision = envParams.GetWithDefault("use_advanced", useAdvancedDecision ? 1f : 0f) > 0f;

        transform.localPosition = new Vector2(Random.Range(-10f, 10f), Random.Range(-5f, 5f));
        ClearTargets();
        SpawnTargets();
        currentTarget = null;
        rewardTimer = 0;
        patienceCounter = 0;
        lastPosition = transform.localPosition;
        _previousDirection = Vector2.zero;
        _stepCount = 0;
    }

    private void SpawnTargets()
    {
        totalTargets = targetCount;

        for (int i = 0; i < targetCount; i++)
        {
            Vector2 pos = new Vector2(Random.Range(-10f, 10f), Random.Range(-5f, 5f));
            DRL_TargetCamper t = Instantiate(targetPrefab, environment);
            t.transform.localPosition = pos;

            if (targetsAreMoving)
                t.Movement?.Begin(transform.position.x > pos.x);

            targets.Add(t);
        }
    }

    private void ClearTargets()
    {
        foreach (var t in targets)
            Destroy(t.gameObject);
        targets.Clear();
    }

    private float Normalize(float val, float min, float max)
    {
        return (val - min) / (max - min);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
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

            sensor.AddObservation(useAdvancedDecision ? currentTarget.HP / currentTarget.MaxHP : 0f);
        }
        else
        {
            sensor.AddObservation(0f);
            sensor.AddObservation(0f);
            sensor.AddObservation(0f);
        }
    }

    private void DebugLog(string message)
    {
        if (debug)
        {
            Debug.Log(message);
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float h = actions.ContinuousActions[0];
        float v = actions.ContinuousActions[1];
        Vector2 movement = new Vector2(h, v) * speed * Time.deltaTime;
        rbody.MovePosition(rbody.position + movement);

        DebugLog($"Action: h = {h}, v = {v}, movement = {movement}");

        // 1. Нагорода за непусту швидкість (щоб не стояв на місці)
        float speedNorm = rbody.velocity.magnitude / speed; // від 0 до 1
        AddReward(0.002f * speedNorm);

        // 2. Штраф за дуже різкі розвороти (маятниковий рух)
        if (_previousDirection != Vector2.zero)
        {
            float cosAngle = Vector2.Dot(_previousDirection, movement.normalized);
            if (cosAngle < 0f) AddReward(-0.02f);
        }
        _previousDirection = movement.normalized;

        rewardTimer += Time.deltaTime;
        if (rewardTimer > 3f)
        {
            AddReward(-0.05f);
            DebugLog("Idle penalty: -0.05");
            rewardTimer = 0f;
            ground.color = Color.yellow; // Change ground color to indicate penalty
        }

        if (Vector2.Distance(transform.localPosition, lastPosition) < 0.05f)
        {
            AddReward(-0.01f);
            DebugLog("Stuck penalty: -0.01");
        }

        lastPosition = transform.localPosition;

        if (currentTarget == null || !targets.Contains(currentTarget) || patienceCounter > 20)
        {
            currentTarget = SelectTarget();
            lastDistanceToTarget = currentTarget != null ? Vector2.Distance(transform.localPosition, currentTarget.transform.localPosition) : 0f;
            patienceCounter = 0;

            DebugLog($"New target selected: {(currentTarget != null ? currentTarget.name : "null")}, initial distance: {lastDistanceToTarget}");
        }

        if (currentTarget != null)
        {
            float dist = Vector2.Distance(transform.localPosition, currentTarget.transform.localPosition);
            float alignment = Vector2.Dot(movement.normalized, (currentTarget.transform.localPosition - transform.localPosition).normalized);
            //if (alignment > 0.8f) AddReward(0.02f);

            if (dist < 0.5f)
            {
                Catch(currentTarget.gameObject);
                DebugLog($"Caught target: {currentTarget?.name}, reward given in Catch()");
            }
            else if (dist < lastDistanceToTarget)
            {
                //AddReward(0.02f);
                //DebugLog("Approaching target: +0.02");
            }
            else
            {
                AddReward(-0.01f);
                DebugLog("Moving away from target: -0.01");
            }

            lastDistanceToTarget = dist;
            patienceCounter++;
        }
    }


    private DRL_TargetCamper SelectTarget()
    {
        DRL_TargetCamper best = null;
        float bestScore = float.MaxValue;

        foreach (var t in targets)
        {
            float dist = Vector2.Distance(transform.localPosition, t.transform.localPosition);
            float score = dist;

            if (useAdvancedDecision)
            {
                score -= t.HP / t.MaxHP; // менше hp → вищий пріоритет
            }

            if (score < bestScore)
            {
                best = t;
                bestScore = score;
            }
        }
        return best;
    }

    private void Catch(GameObject target)
    {
        AddReward(1f);

        float timeFactor = (maxEpisodeSteps - _stepCount) / (float)maxEpisodeSteps;
        AddReward(0.5f * Mathf.Clamp01(timeFactor));

        ground.color = Color.cyan;
        targets.Remove(target.GetComponent<DRL_TargetCamper>());
        Destroy(target);

        if (targets.Count == 0)
        {
            ground.color = Color.green;
            AddReward(3f);
            EndEpisode();
        }

        currentTarget = null;
        patienceCounter = 0;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var ca = actionsOut.ContinuousActions;
        ca[0] = Input.GetAxis("Horizontal");
        ca[1] = Input.GetAxis("Vertical");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(targetTag))
        {
            return;
            DebugLog($"Caught target: {collision.gameObject.name}");
            Catch(collision.gameObject);
        }

        if (collision.CompareTag("Walls"))
        {
            DebugLog("Hit a wall!");
            ground.color = Color.red;
            AddReward(-3f);
            ClearTargets();
            EndEpisode();
        }
    }
}
