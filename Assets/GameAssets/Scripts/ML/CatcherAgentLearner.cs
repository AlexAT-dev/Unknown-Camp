using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class CatcherAgentLearner : Agent
{
    [Header("Agent")]
    [SerializeField] private float speed = 8f;
    private Rigidbody2D rbody;
    [SerializeField] private string targetTag = "PlayerCamper";

    [Header("Learning")]
    [SerializeField] private int targetsCountMin = 1;
    [SerializeField] private int targetsCountMax = 4;
    [SerializeField] private GameObject targetPrefab;
    [SerializeField] private List<GameObject> targets;
    //[SerializeField] private bool isLearning = true;

    [Header("Walls")]
    [SerializeField] private Vector2 minPosition;
    [SerializeField] private Vector2 maxPosition;

    private float targetsInitCount;
    private GameObject currentTarget; // Holds the target the agent is focusing on
    private float lastDistanceToTarget; // Used to reward movement toward the target
    private int patienceCounter = 0;
    private Vector3 lastPosition; // Track position for oscillation detection

    // Timer for periodic reward reduction
    private float rewardReductionTimer = 0f;
    [SerializeField] private float rewardReductionInterval = 2.5f; // Time in seconds before periodic reward reduction
    [SerializeField] private float patienceLimit = 20; // Number of steps to "stick" to a target

    [Header("Environment")]
    [SerializeField] private Transform environment;
    [SerializeField] private SpriteRenderer ground;

    public override void Initialize()
    {
        rbody = GetComponent<Rigidbody2D>();
    }

    public override void OnEpisodeBegin()
    {
        bool side = Random.Range(0, 10) > 2;
        transform.localPosition = side switch
        {
            true => new Vector2(Random.Range(2f, 14f), Random.Range(-5.5f, 5.5f)),
            _ => new Vector2(Random.Range(-14f, -2f), Random.Range(-5.5f, 5.5f))
        };

        CreateTargets(Random.Range(targetsCountMin, targetsCountMax + 1), side);

        rewardReductionTimer = 0f;
        currentTarget = null;
        patienceCounter = 0;
        lastPosition = transform.localPosition;
    }

    private void ClearTargets()
    {
        foreach (GameObject obj in targets)
        {
            Destroy(obj);
        }
        targets.Clear();
    }

    private void CreateTargets(int targetsCount, bool side)
    {
        targetsInitCount = targetsCount;
        ClearTargets();

        for (int i = 0; i < targetsCount; i++)
        {
            GameObject target = Instantiate(targetPrefab, environment);

            target.transform.localPosition = side switch
            {
                false => new Vector2(Random.Range(0f, 2f), Random.Range(-5.5f, 5.5f)),
                _ => new Vector2(Random.Range(-2f, 0f), Random.Range(-5.5f, 5.5f))
            };

            target.GetComponent<ML_MovingTargetRandom>().Begin(side);
            targets.Add(target);
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition.x);
        sensor.AddObservation(transform.localPosition.y);

        if (currentTarget != null)
        {
            sensor.AddObservation(currentTarget.transform.localPosition.x);
            sensor.AddObservation(currentTarget.transform.localPosition.y);
        }
        else
        {
            //Placeholders
            sensor.AddObservation(0f);
            sensor.AddObservation(0f);
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveY = actions.ContinuousActions[1];

        Vector2 move = new Vector2(moveX, moveY) * speed * Time.deltaTime;
        transform.localPosition += (Vector3)move;
        rbody.MovePosition(rbody.position + move);

        rewardReductionTimer += Time.deltaTime;
        if (rewardReductionTimer > rewardReductionInterval)
        {
            AddReward(-0.05f); // Small periodic penalty
        }

        if (rewardReductionTimer > rewardReductionInterval * 4)
        {
            AddReward(-10f);
            EndEpisode();
            ground.color = Color.red;
            rewardReductionTimer = 0f;
        }

        // oscillation
        if ((transform.localPosition - lastPosition).magnitude < 0.01f)
        {
            AddReward(-0.005f); // Small penalty for not making progress
        }
        lastPosition = transform.localPosition;

        if (currentTarget == null || !targets.Contains(currentTarget) || patienceCounter >= patienceLimit)
        {
            currentTarget = GetClosestTarget();
            if (currentTarget != null)
            {
                lastDistanceToTarget = Vector2.Distance(transform.localPosition, currentTarget.transform.localPosition);
            }
            patienceCounter = 0;
        }

        if (currentTarget != null)
        {
            float distanceToTarget = Vector2.Distance(transform.localPosition, currentTarget.transform.localPosition);

            if (distanceToTarget < 1f)
            {
                Catch(currentTarget);
            }

            patienceCounter++;
        }
    }

    private void Catch(GameObject target)
    {
        targets.Remove(target.gameObject);
        Destroy(target.gameObject);
        ground.color = Color.cyan;
        AddReward(20f / targetsInitCount);

        // Reset values
        currentTarget = null;
        rewardReductionTimer = 0f;
        patienceCounter = 0;

        if (targets.Count == 0)
        {
            ground.color = Color.green;
            AddReward(5f);
            EndEpisode();
        }

        CamperAgentLearner camper = target.GetComponent<CamperAgentLearner>();
        if (camper) camper.GetCatched();
    }

    private GameObject GetClosestTarget()
    {
        GameObject closest = null;
        float closestDistance = float.MaxValue;

        foreach (GameObject target in targets)
        {
            PlayerCamper playerCamper = target.GetComponent<PlayerCamper>();
            if (playerCamper != null && playerCamper.IsHiding) continue;

            float distance = Vector2.Distance(transform.localPosition, target.transform.localPosition);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = target;
            }
        }
        return closest;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(targetTag))
        {
            Catch(collision.gameObject);
        }

        if (collision.CompareTag("Walls"))
        {
            ground.color = Color.red;
            AddReward(-5f);
            EndEpisode();
        }
    }
}
