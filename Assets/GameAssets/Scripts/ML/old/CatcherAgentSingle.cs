using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class CatcherAgentSingle : Agent
{
    public SpriteRenderer ground;
    public Transform target;
    public float speed = 5f;

    public override void OnEpisodeBegin()
    {
        transform.localPosition = new Vector2(Random.Range(-5f, 5f), Random.Range(-3f, 3f));
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition.x);
        sensor.AddObservation(transform.localPosition.y);
        sensor.AddObservation(target.localPosition.x);
        sensor.AddObservation(target.localPosition.y);
    }

    // Define actions for the agent
    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveY = actions.ContinuousActions[1];

        Vector2 move = new Vector2(moveX, moveY) * speed * Time.deltaTime;
        transform.localPosition += (Vector3)move;

        float distanceToTarget = Vector2.Distance(transform.position, target.position);
        if (distanceToTarget < 1f)
        {
            ground.color = Color.green;
            SetReward(5.0f); 
            EndEpisode();
        }
        else
        {
            SetReward(-0.01f); 
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Walls")
        {
            ground.color = Color.red;
            SetReward(-2.0f);
            EndEpisode();
        }
        else if (collision.tag == "PlayerCamper")
        {
            ground.color = Color.green;
            SetReward(5.0f);
            EndEpisode();
        }
    }
}
