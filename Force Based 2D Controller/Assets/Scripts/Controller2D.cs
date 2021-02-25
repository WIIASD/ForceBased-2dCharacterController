using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Player))]

/// <summary>
/// Learned from GDC: https://youtu.be/YDwp5tNCKso for the force calculation part
/// </summary>
public class Controller2D : MonoBehaviour
{
    public float speedModifier = 3f;
    public float jumpStrength = 15f;
    public float basicJumpStrength = 3f;
    public float wallJumpHorizontalStrength = 4f;
    public float wallJumpVerticalStrength = 10f;
    public float timeToReachJumpApex = 0.2f;
    public bool isJumping = false;
    public bool apexReached = false;

    private int jumpBoost = 0;
    private float jumpW;
    private Player player;
    
    BoxCollider2D boxCollider;

    [SerializeField]

    List<Collider2D> colliderStandedOn = new List<Collider2D>();

    [SerializeField]

    List<Collider2D> colliderWalledLeft = new List<Collider2D>();

    [SerializeField]

    List<Collider2D> colliderWalledRight = new List<Collider2D>();

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
    }

    private void FixedUpdate()
    {
        HandleMove();
        HandleJump();
    }

    public bool IsGrounded(){
        return colliderStandedOn.Count < 1 ? false : true;
    }

    public bool IsWalledLeft(){
        return colliderWalledLeft.Count < 1 ? false : true;
    }

    public bool IsWalledRight(){
        return colliderWalledRight.Count < 1 ? false : true;
    }

    public int GetWallJumpDirection()
    {
        if (colliderStandedOn.Count != 0) 
        {
            return 0;
        }
        if (colliderWalledLeft.Count != 0) 
        {
            return 1;
        }
        if (colliderWalledRight.Count != 0)
        {
            return -1;
        }
        return 0;
    }

    public void Jump(){
        
        if(!IsGrounded() && !IsWalledRight() && !IsWalledLeft()){//before jump
            return;
        }

        int wallJumpDirection = GetWallJumpDirection();

        if (wallJumpDirection == 0) //normal jump
        {
            rb2d.velocity = new Vector2(rb2d.velocity.x, basicJumpStrength);
            isJumping = true;
        }
        else
        {
            rb2d.velocity = new Vector2(rb2d.velocity.x + wallJumpDirection * wallJumpHorizontalStrength, rb2d.velocity.y / 2 + wallJumpVerticalStrength);
            isJumping = true;
        }
        
    }

    public void StopJump(){
        isJumping = false;
        apexReached = false;
        jumpBoost = 0;
    }

    /// <summary>
    /// move the player by adding a force every FixedUpdate frames
    /// </summary>
    void HandleMove()
    {
        Vector2 force = new Vector2(speedModifier * player.horaxis * rb2d.mass, 0);
        rb2d.AddForce(force);
    }
    /// <summary>
    /// Jump by adding a force every FixedUpdate frames
    /// /// </summary>
    void HandleJump()
    {
        if (isJumping)
        {
            //this is an counter that keep track of the time in air in every FixedUpdate frames (0.02s)
            //will be reset after landing
            jumpBoost++;
            float C = Mathf.Cos(jumpW * jumpBoost);
            if (C < 0f)
            {
                apexReached = true;
                StopJump();
            }
            else
            {
                rb2d.velocity += new Vector2(0f, C * jumpStrength);
            }
        }
    }


    /// <summary>
    /// The following 3 methods handle collision to ground with different normal so that the player knows what kind of wall(s)/ground(s) is colliding.
    /// I used arrays to store different gameobjects that are colliding with the player
    /// </summary>
    /// <param name="other"></param>
    private void OnCollisionEnter2D(Collision2D other)
    {
        ContactPoint2D contactPoint = other.GetContact(0);

        if (other.collider.CompareTag("Ground")) {

            if (contactPoint.normal.y > 0.5f) //ground
            {
                //contactPoint.collider.GetComponent<SpriteRenderer>().color = Color.red;
                colliderStandedOn.Add(contactPoint.collider);
            }

            if (contactPoint.normal.x > 0.8f)//left wall
            {
                //contactPoint.collider.GetComponent<SpriteRenderer>().color = Color.green;
                colliderWalledLeft.Add(contactPoint.collider);
            }

            if (contactPoint.normal.x < -0.8f)//right wall
            {
                //contactPoint.collider.GetComponent<SpriteRenderer>().color = Color.blue;
                colliderWalledRight.Add(contactPoint.collider);
            }
        }
        
    }

    private void OnCollisionExit2D(Collision2D other)
    {

        if (colliderStandedOn.Count > 0)
        {
            if (colliderStandedOn.Contains(other.collider))
            {
                //other.collider.GetComponent<SpriteRenderer>().color = Color.white;
                colliderStandedOn.Remove(other.collider);
            }
        }

        if (colliderWalledLeft.Count > 0)
        {
            if (colliderWalledLeft.Contains(other.collider))
            {
                //other.collider.GetComponent<SpriteRenderer>().color = Color.white;
                colliderWalledLeft.Remove(other.collider);
            }
        }

        if (colliderWalledRight.Count > 0)
        {
            if (colliderWalledRight.Contains(other.collider))
            {
                //other.collider.GetComponent<SpriteRenderer>().color = Color.white;
                colliderWalledRight.Remove(other.collider);
            }
        }

    }

    /// <summary>
    /// To double check if there are unexpected colliders that the player is not colliding with in the collider arrays 
    /// </summary>
    /// <param name="other"></param>
    private void OnCollisionStay2D(Collision2D other)
    {
        ContactPoint2D contactPoint = other.GetContact(0);
        Debug.Log(contactPoint.normal.y);
        if (Mathf.Abs(contactPoint.normal.x) < 0.8 &&
            (colliderWalledLeft.Contains(other.collider) || colliderWalledRight.Contains(other.collider)))
        {
            colliderWalledLeft.Remove(other.collider);
            colliderWalledRight.Remove(other.collider);
            if (contactPoint.normal.y > 0.5)
            {
                colliderStandedOn.Add(other.collider);
                //contactPoint.collider.GetComponent<SpriteRenderer>().color = Color.red;
            }
        }
    }


}
