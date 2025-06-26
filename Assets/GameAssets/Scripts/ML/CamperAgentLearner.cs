using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class CamperAgentLearner : Agent
{
    [Header("Agent")]
    [SerializeField] private float speed = 4f;
    private Rigidbody2D rbody;
    private GameObject catcher;
    public override void Initialize()
    {
        rbody = GetComponent<Rigidbody2D>();
    }

    public void Created(GameObject catcher)
    {
        this.catcher = catcher;
    }


    public override void OnEpisodeBegin()
    {
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition.x);
        sensor.AddObservation(transform.localPosition.y);

        sensor.AddObservation(catcher.transform.localPosition.x);
        sensor.AddObservation(catcher.transform.localPosition.y);
    }

    public void GetCatched()
    {
        AddReward(-10.0f);
        EndEpisode();
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveY = actions.ContinuousActions[1];

        Vector2 move = new Vector2(moveX, moveY) * speed * Time.deltaTime;
        transform.localPosition += (Vector3)move;
        rbody.MovePosition(rbody.position + move);

        AddReward(0.25f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Walls"))
        {
            AddReward(-20.0f);
            EndEpisode();
        }
    }
}
