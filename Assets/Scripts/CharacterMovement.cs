using UnityEngine;
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

    public void HandleMovement(float posX, float posY)
    {
        //This doesn't work properly.. just moves to the left jerkingly
        Vector2 movementVector = Vector2.MoveTowards(transform.position, 
            new Vector2(posX, posY), speed * Time.deltaTime);

        if (movementVector.x != 0)
        {
            IsMoving = true;
            animator.SetBool("isWalking", IsMoving);

            animator.SetFloat("input_x", movementVector.x);
        }

        if (movementVector.y != 0)
        {
            IsMoving = true;
            animator.SetBool("isWalking", IsMoving);
            animator.SetFloat("input_y", movementVector.y);
        }

        if (movementVector == Vector2.zero)
        {
            IsMoving = false;
            animator.SetBool("isWalking", IsMoving);
        }
        rb.MovePosition(movementVector);
        //diagonal input fails at times
        //moving diagonally moves faster
    }
}
