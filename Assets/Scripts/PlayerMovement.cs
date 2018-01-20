using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

    private Rigidbody2D rb;
    private Animator animator;
    public float speed;
    [HideInInspector]
    public bool IsMoving { get; set; }

    // Use this for initialization
    void Start () {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        Vector2 movementVector = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

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
        }

        if(movementVector == Vector2.zero)
        {
            IsMoving = false;
            animator.SetBool("isWalking", IsMoving);
        }
        rb.MovePosition(rb.position + (speed * movementVector.normalized * Time.deltaTime));
        //diagonal input fails at times
        //moving diagonally moves faster
	}
}
