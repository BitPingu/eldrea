using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCMovement : MonoBehaviour
{
    public float maxSpeed; // default is 3f 3 3 2
    private float moveSpeed;
    private Vector2 movement;

    private SpriteRenderer sprite;
    private Rigidbody2D rb;
    private Animator animator;

    [SerializeField]
    private Vector2Int currentPos;
    [SerializeField]
    private int currentTile;
    private bool isMoving;

    [SerializeField]
    private float walkTime; // default is 1f 1 .5 1
    private float walkCounter;
    private float waitTime;
    private float waitCounter;
    [SerializeField]
    private float minWaitTime; // default is 3f
    [SerializeField]
    private float maxWaitTime; // default is 5f

    private void Awake()
    {
        // Retrieve components of enemy
        sprite = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // Choose direction
        chooseDirection();

        if (GetComponent<EnemyBattle>())
            GetComponent<EnemyBattle>().enabled = false;
    }

    // Update is called once per frame
    private void Update()
    {
        // Retrieve coordinates of enemy
        currentPos = Vector2Int.FloorToInt(transform.position);

        // Get speed
        moveSpeed = maxSpeed;

        if (moveSpeed <= 0)
            moveSpeed = 1;

        // look at player when nearby and chase it
        if (GetComponent<EnemyPosition>() && GetComponent<EnemyData>().isHostile && GetComponent<EnemyPosition>().CheckPlayer())
        {
            Chase();
        }
        else
        {
            if (isMoving)
            {
                // Update walk counter
                walkCounter -= Time.deltaTime;

                // Movement
                Vector2 moveForce = movement * (moveSpeed+.5f);
                moveForce /= 1.2f;
                rb.velocity = moveForce;

                // Flip sprite based on horizontal movement
                if (movement.x > 0)
                {
                    sprite.flipX = false;
                }
                else if (movement.x < 0)
                {
                    sprite.flipX = true;
                }

                // Stop moving
                if (walkCounter < 0)
                {
                    isMoving = false;

                    // Reset wait time
                    waitCounter = waitTime;
                }
            }
            else
            {
                // Stop moving
                rb.velocity = Vector2.zero;

                // Update wait counter
                waitCounter -= Time.deltaTime;

                // Choose direction
                if (waitCounter < 0)
                {
                    chooseDirection();
                }
            }
        }

        // Movement animation
        animator.SetFloat("Speed", rb.velocity.sqrMagnitude);
    }

    private void chooseDirection()
    {
        // Choose random movement
        movement.x = Random.Range(-1f, 1f);
        movement.y = Random.Range(-1f, 1f);

        // Choose random wait time
        waitTime = Random.Range(minWaitTime, maxWaitTime);
        
        // Start moving
        isMoving = true;

        // Reset walk time
        walkCounter = walkTime;
    }

    private void Chase()
    {
        // Calculate current direction towards player
        Vector2 movement = (FindObjectOfType<PlayerPosition>().transform.position - rb.transform.position).normalized;

        // Move towards player
        Vector2 moveForce = movement * (moveSpeed+1);
        moveForce /= 1.2f;
        rb.velocity = moveForce;

        // Flip sprite based on horizontal movement
        if (movement.x < 0)
        {
            sprite.flipX = true;
        }
        else
        {
            sprite.flipX = false;
        }
    }
}
