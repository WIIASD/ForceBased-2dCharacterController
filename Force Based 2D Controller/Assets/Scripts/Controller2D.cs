using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Player))]

/// <summary>
/// Learned from GDC: https://youtu.be/YDwp5tNCKso
/// </summary>
public class Controller2D : MonoBehaviour
{
    Player player;
    public bool jumpPressed, jumpReleased;
    public float speedModifier = 3f;
    public float jumpStrength = 15f;
    public float basicJumpStrength = 3f;
    public float timeToReachJumpApex = 0.2f;
    float jumpW;
    int jumpBoost = 0;
    float horaxis = 0f;
    BoxCollider2D boxCollider;
    List<Collider2D> colliderStandedOn = new List<Collider2D>();
    List<Collider2D> colliderWalled = new List<Collider2D>();
    Rigidbody2D rb2d;

    private void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        rb2d = GetComponent<Rigidbody2D>();
        player = GetComponent<Player>();
        jumpW = (Mathf.PI / 2) / (timeToReachJumpApex / 0.02f); // Calculate jumpW based on timeToReachApex
    }

    private void Update()
    {
        updateInput();
        updatePlayerInfo();
    }
    private void FixedUpdate()
    {
        move();
        jump();
    }
    /// <summary>
    /// move the player by adding a force every Fixed Update
    /// </summary>
    void move()
    {
        Vector2 force = new Vector2(speedModifier * horaxis * rb2d.mass, 0);
        rb2d.AddForce(force);
    }
    /// <summary>
    /// Jump by adding a force every Fixed Update
    /// </summary>
    void jump()
    {
        if (player.isJumping)
        {
            //this is an counter that keep track of the time in air
            //will be reset after landing
            jumpBoost++;
            float C = Mathf.Cos(jumpW * jumpBoost);
            if (C < 0f)
            {
                player.apexReached = true;
            }
            else
            {
                rb2d.velocity += new Vector2(0f, C * jumpStrength);
            }
        }
    }
    /// <summary>
    /// Update input from player every frame
    /// </summary>
    void updateInput()
    {
        horaxis = Input.GetAxisRaw("Horizontal");

        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpReleased = false;
            jumpPressed = true;
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            jumpPressed = false;
            jumpReleased = true;
        }
    }
    /// <summary>
    /// Update player status information every frame
    /// </summary>
    void updatePlayerInfo()
    {
        player.grounded = colliderStandedOn.Count < 1 ? false : true;

        player.walled = colliderWalled.Count < 1 ? false : true;

        if (jumpPressed && player.grounded)
        {
            player.grounded = false;
            player.isJumping = true;
            jumpPressed = false;
            rb2d.velocity = new Vector2(rb2d.velocity.x, basicJumpStrength);
        }
        if (jumpReleased || player.apexReached)
        {
            player.isJumping = false;
            player.apexReached = false;
            jumpBoost = 0;
        }
    }
    /// <summary>
    /// The following 3 methods handle collision to ground with different normal
    /// so that the player knows what kind of wall(s)/ground(s) 
    /// is colliding.
    /// I used arrays to store different gameobjects that
    /// are colliding with the player
    /// </summary>
    /// <param name="other"></param>
    private void OnCollisionEnter2D(Collision2D other)
    {
        ContactPoint2D contactPoint = other.GetContact(0);
        if (contactPoint.normal.y > 0.5f && other.collider.CompareTag("Ground"))
        {
            contactPoint.collider.GetComponent<SpriteRenderer>().color = Color.red;
            colliderStandedOn.Add(contactPoint.collider);
        }

        if (contactPoint.normal.x > 0.8f && other.collider.CompareTag("Ground"))
        {
            contactPoint.collider.GetComponent<SpriteRenderer>().color = Color.blue;
            colliderWalled.Add(contactPoint.collider);
            player.wallJumpDirection = 1;
        }

        if (contactPoint.normal.x < -0.8f && other.collider.CompareTag("Ground"))
        {
            contactPoint.collider.GetComponent<SpriteRenderer>().color = Color.blue;
            colliderWalled.Add(contactPoint.collider);
            player.wallJumpDirection = -1;
        }
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        if (colliderStandedOn.Count > 0)
        {
            if (colliderStandedOn.Contains(other.collider))
            {
                other.collider.GetComponent<SpriteRenderer>().color = Color.white;
                colliderStandedOn.Remove(other.collider);
            }
        }
        if (colliderWalled.Count > 0)
        {
            if (colliderWalled.Contains(other.collider))
            {
                other.collider.GetComponent<SpriteRenderer>().color = Color.white;
                colliderWalled.Remove(other.collider);
                player.wallJumpDirection = 0;
            }
        }
    }

    private void OnCollisionStay2D(Collision2D other)
    {
        ContactPoint2D contactPoint = other.GetContact(0);
        if (Mathf.Abs(contactPoint.normal.x) < 0.8 && colliderWalled.Contains(other.collider))
        {
            colliderWalled.Remove(other.collider);
            if (contactPoint.normal.y > 0.5)
            {
                colliderStandedOn.Add(other.collider);
                contactPoint.collider.GetComponent<SpriteRenderer>().color = Color.red;
            }
        }
    }


}
