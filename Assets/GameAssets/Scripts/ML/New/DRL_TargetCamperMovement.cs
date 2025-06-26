using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DRL_TargetCamperMovement : MonoBehaviour
{
    [SerializeField] private Animator animator;

    public bool auto = false;

    public float speed = 2f;
    public float minX = -14f;
    public float maxX = 14f;
    public float minY = -5f;
    public float maxY = 5f;
    public int minPoints = 3;
    public int maxPoints = 10;


    private List<Vector2> points;
    private int currentPointIndex = 0;
    private bool isMoving = false;
    private void Start()
    {
        if (auto) Begin(true);
    }

    public void Begin(bool side)
    {
        //if (side)
        //{
        //    minX = -15f;
        //    maxX = -2f;
        //}
        //else
        //{
        //    minX = 2f;
        //    maxX = 15f;
        //}

        animator.SetBool("moving", true);
        GenerateRandomPoints();
        UpdateDirection();
        isMoving = true;
    }

    private void FixedUpdate()
    {
        if (!isMoving) return;

        MoveTowardsCurrentPoint();
    }

    private void GenerateRandomPoints()
    {
        currentPointIndex = 0;
        int numPoints = Random.Range(minPoints, maxPoints);
        points = new List<Vector2>();

        for (int i = 0; i < numPoints; i++)
        {
            float randomX = Random.Range(minX, maxX);
            float randomY = Random.Range(minY, maxY);
            points.Add(new Vector2(randomX, randomY));
        }
    }

    private void UpdateDirection()
    {
        transform.localScale = new Vector3(transform.localPosition.x < points[currentPointIndex].x ? -1 : 1, 1, 1);
    }

    private void MoveTowardsCurrentPoint()
    {
        if (points.Count == 0) return;

        Vector2 targetPoint = points[currentPointIndex];
        Vector2 currentlocalPosition = transform.localPosition;
        Vector2 direction = (targetPoint - currentlocalPosition).normalized;
        float step = speed * Time.deltaTime;

        transform.localPosition = Vector2.MoveTowards(currentlocalPosition, targetPoint, step);

        if (Vector2.Distance(currentlocalPosition, targetPoint) < 0.1f)
        {
            currentPointIndex = (currentPointIndex + 1) % points.Count;
            UpdateDirection();
        }
    }
}
