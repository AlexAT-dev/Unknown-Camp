using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class CatcherAgentSingleLearner : Agent
{
    public SpriteRenderer ground;
    public Transform target;
    public float speed = 8f; // Movement speed of the agent

    // Reset the environment
    public override void OnEpisodeBegin()
    {
        // Randomly reset the agent's and target's position on the 2D plane
        transform.localPosition = new Vector2(Random.Range(-5f, 5f), Random.Range(-3f, 3f));
        target.localPosition = new Vector2(Random.Range(-5f, 5f), Random.Range(-3f, 3f));
        //target.GetComponent<ML_MovingTargetRandom>().Begin(true);
    }

    // Collect observations for the agent
    public override void CollectObservations(VectorSensor sensor)
    {
        // Observe the position of the target relative to the agent in 2D
        sensor.AddObservation(transform.localPosition.x);
        sensor.AddObservation(transform.localPosition.y);
        sensor.AddObservation(target.localPosition.x);
        sensor.AddObservation(target.localPosition.y);
    }

    // Define actions for the agent
    public override void OnActionReceived(ActionBuffers actions)
    {
        // 2D Movement actions (continuous)
        float moveX = actions.ContinuousActions[0];
        float moveY = actions.ContinuousActions[1];

        // Calculate movement vector and apply it
        Vector2 move = new Vector2(moveX, moveY) * speed * Time.deltaTime;
        transform.localPosition += (Vector3)move;

        // Reward based on distance to the target
        float distanceToTarget = Vector2.Distance(transform.position, target.position);
        if (distanceToTarget < 1f)
        {
            ground.color = Color.green;
            SetReward(5.0f); // Reward for reaching the target
            EndEpisode();
        }
        else
        {
            SetReward(-0.01f); // Small penalty to encourage faster completion
        }
    }

    // Optional: heuristic for manual control testing
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
    }
}
