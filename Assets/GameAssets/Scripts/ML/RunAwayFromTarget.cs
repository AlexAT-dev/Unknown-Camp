using UnityEngine;

using System.Collections;
using System.Collections.Generic;

public class RunAwayFromTarget : MonoBehaviour
{
    [SerializeField] private Animator animator;

    public Transform target;       // Target to run away from
    public float speed = 2f;
    public float runAwaySpeed = 5f; // Speed when running away
    public float safeDistance = 3f; // Distance at which to start running away
    public float minX = -5f;
    public float maxX = 5f;
    public float minY = -5f;
    public float maxY = 5f;
    public int minPoints = 3;
    public int maxPoints = 10;

    private List<Vector2> points;
    private int currentPointIndex = 0;

    public void Created(Transform target)
    {
        this.target = target;
    }

    public void Begin()
    {
        animator.SetBool("moving", true);
        GenerateRandomPoints();
        UpdateDirection();
    }

    private void Start()
    {
        Begin();
    }

    private void Update()
    {
        if (target != null && Vector2.Distance(transform.position, target.position) < safeDistance)
        {
            RunAway();
        }
        else
        {
            MoveTowardsCurrentPoint();
        }
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
        if (points.Count > 0)
        {
            transform.localScale = new Vector3(transform.localPosition.x < points[currentPointIndex].x ? -1 : 1, 1, 1);
        }
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

    private void RunAway()
    {
        // Calculate direction away from the target
        Vector3 directionAway = transform.position - target.position;
        directionAway.Normalize();

        // Move object away from target
        transform.position += directionAway * runAwaySpeed * Time.deltaTime;
    }
}
