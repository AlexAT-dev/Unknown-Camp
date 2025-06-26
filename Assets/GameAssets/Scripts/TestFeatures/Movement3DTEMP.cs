using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement3DTEMP : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField, ReadOnly] private Vector3 direction;
    [SerializeField, ReadOnly] private bool isMoving;
    [SerializeField] private Rigidbody rbody;
    [SerializeField] private Animator animator;
    public bool IsMoving => isMoving;

    private void Update()
    {
        Move();
    }

    public void Move()
    {
        direction = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized;
        
        isMoving = direction != Vector3.zero;
        animator?.SetBool("moving", isMoving);

        animator.gameObject.transform.localScale = new Vector3(direction.x > 0 ? -1 : 1, 1, 1);
        rbody.velocity = direction * (direction == Vector3.zero ? 1 : speed);

    }
}
