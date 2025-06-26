using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Linq;

public class CatcherAgent : Agent
{
    [Header("Agent Settings")]
    [SerializeField] private float speed = 8f;
    private Rigidbody2D rbody;

    [SerializeField, ReadOnly] private GameObject currentTarget;
    private List<GameObject> targets;
    private List<GameObject> remainTargets;

    private void Start()
    {
        if (GameController.Instance != null && !GameController.IControllAI)
        {
            Destroy(gameObject);
        }
    }

    public override void Initialize()
    {
        rbody = GetComponent<Rigidbody2D>();
        targets = GameObject.FindGameObjectsWithTag("PlayerCamper").ToList();
    }

    public override void OnEpisodeBegin()
    {
        remainTargets = new List<GameObject>(targets);
        transform.localPosition = new Vector2(Random.Range(-24.4f, 17f), Random.Range(-34f, 17f));
        currentTarget = GetClosestTarget();

        foreach (GameObject target in targets)
        {
            target.transform.localPosition = new Vector2(Random.Range(-24.4f, 17f), Random.Range(-34f, 17f));
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

        // Update current target if needed
        if (currentTarget == null)
        {
            currentTarget = GetClosestTarget();
        }
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


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PlayerCamper"))
        {
            collision.transform.position = new Vector3(0, 100, 0);
            remainTargets.Remove(collision.gameObject);
            currentTarget = GetClosestTarget();

            if (remainTargets.Count == 0)
            {
                EndEpisode();
            }
        }
    }
}
