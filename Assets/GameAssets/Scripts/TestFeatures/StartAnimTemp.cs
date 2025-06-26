using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartAnimTemp : MonoBehaviour
{
    public Animator animator;
    public string anim;
    void Start()
    {
        animator.Play(anim);
    }
}
