using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DRL_TargetCamper : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private DRL_TargetCamperMovement movement;
    [SerializeField] private CharacterCamper character;
    [SerializeField] private CharactersList charactersList;

    [Header("Data")]
    [SerializeField] private float speed = 0f;
    [SerializeField] private int hp = 3;

    public int MaxHP => character.Traits.hp;
    public int HP => hp;

    public DRL_TargetCamperMovement Movement => movement;
    public CharacterCamper Character => character;

    public void Initialize(float speed = 0, int hp = 0)
    {
        if (movement == null)
            movement = GetComponent<DRL_TargetCamperMovement>();
        
        character = charactersList.DefaultCamper;

        this.hp = (hp <= 0f) ? Random.Range(1, MaxHP + 1) : hp;
        this.speed = (speed <= 0f) ? character.Traits.speed : speed;
        movement.speed = this.speed;
    }
}
