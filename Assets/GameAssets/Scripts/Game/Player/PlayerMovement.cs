using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private float speed => player.Character.speed + SpeedMultiplayer;
    [SerializeField] private Player player;
    [SerializeField] private List<string> blockReasons;
    [SerializeField] private Dictionary<string, float> speedMultiplayers = new();
    [SerializeField, ReadOnly] private Vector2 direction;
    [SerializeField, ReadOnly] private bool isMoving;

    private FixedJoystick Joystick => GameController.Joystick;

    public bool IsBlocked => blockReasons.Count > 0;
    public bool IsMoving => isMoving;
    public float SpeedMultiplayer
    {
        get
        {
            float multspeed = 0f;
            foreach(var item in speedMultiplayers)
            {
                multspeed += item.Value;
            }
            return multspeed;
        }
    }

    public void Move()
    {
        if (IsBlocked) return;

        direction = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized;
        if (direction == Vector2.zero)
            direction = new Vector2(Joystick.Horizontal, Joystick.Vertical).normalized;
        
        isMoving = direction != Vector2.zero;
        if(player.Animator) player.Animator?.SetBool("moving", isMoving);
        player.Rigidbody.velocity = direction * (direction == Vector2.zero ? 1 : speed);
        if(direction.x != 0)
        {
            player.UpdateVisualDir(direction.x > 0);
        }
    }

    public void AddBlockReason(string reason)
    {
        if(!blockReasons.Contains(reason))
        {
            blockReasons.Add(reason);
        }
        player.Rigidbody.velocity = Vector2.zero;
    }

    public void RemoveBlockReason(string reason)
    {
        blockReasons.Remove(reason);
    }

    public void AddSpeedMultiplayer(string reason, float speed)
    {
        speedMultiplayers[reason] = speed;
    }

    public void RemoveSpeedMultiplayer(string reason)
    {
        speedMultiplayers.Remove(reason);
    }
}
