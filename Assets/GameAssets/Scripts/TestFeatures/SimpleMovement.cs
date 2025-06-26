using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleMovement : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField, ReadOnly] private Vector2 direction;
    [SerializeField, ReadOnly] private bool isMoving;
    [SerializeField] private Rigidbody2D rbody;
    [SerializeField] private Animator animator;
    public bool IsMoving => isMoving;

    private void Update()
    {
        Move();
    }

    public void Move()
    {
        direction = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized;
        
        isMoving = direction != Vector2.zero;
        animator?.SetBool("moving", isMoving);

        animator.gameObject.transform.localScale = new Vector3(direction.x > 0 ? -1 : 1, 1, 1);
        rbody.velocity = direction * (direction == Vector2.zero ? 1 : speed);
    }
}
