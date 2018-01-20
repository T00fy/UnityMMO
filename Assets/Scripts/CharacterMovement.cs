﻿using UnityEngine;
using System.Collections;
using System;

[RequireComponent(typeof(CharacterPositionPoller))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class CharacterMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator animator;
    private CharacterPositionPoller poller;
    public float speed = 7.5f;
    public bool IsMoving { get; set; }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        poller = GetComponent<CharacterPositionPoller>();
    }

    void FixedUpdate()
    {
        animator.SetBool("isWalking", IsMoving);
    }

    public void HandleMovement(float posX, float posY)
    {
        Vector2 movementVector = Vector2.Lerp(transform.position, 
           new Vector2(posX, posY), speed * Time.deltaTime);

        if (movementVector.x != 0)
        {
            IsMoving = true;
        //    animator.SetBool("isWalking", IsMoving);

            animator.SetFloat("input_x", movementVector.x);
        }

        if (movementVector.y != 0)
        {
            IsMoving = true;
            //       animator.SetBool("isWalking", IsMoving);
            animator.SetFloat("input_x", movementVector.x);
        }

        rb.MovePosition(movementVector);
        //diagonal input fails at times
        //moving diagonally moves faster
    }
}