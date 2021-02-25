using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Learned from GDC: https://youtu.be/YDwp5tNCKso for the force calculation part
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Player))]
public class Controller2D : MonoBehaviour
{
    public struct RaycastOrigins
    {
        public Vector3 TopLeft, TopRight, BottomLeft, BottomRight;
    }
    [SerializeField] private float rayLength = 0.1f;
    [SerializeField] private int horizontalRayCount = 8;
    [SerializeField] private int verticalRayCount = 8;
    [SerializeField] private float speedModifier = 3f;
    [SerializeField] private float jumpStrength = 15f;
    [SerializeField] private float basicJumpStrength = 3f;
    [SerializeField] private float timeToReachJumpApex = 0.2f;
    [SerializeField] private float wallJumpHorizontalStrength = 4f;
    [SerializeField] private float wallJumpVerticalStrength = 10f;
    [SerializeField] private float skinWidth = 0.02f;
    [SerializeField] private bool isGrounded;
    [SerializeField] private bool isCeilinged;
    [SerializeField] private bool isLeftWalled;
    [SerializeField] private bool isRightWalled;
    [SerializeField] private LayerMask groundLayerMask;
    [SerializeField] private List<Collider2D> colliderWalledLeft, colliderStandedOn, colliderWalledRight = new List<Collider2D>();

    private RaycastOrigins raycastOrigins;
    private float horizontalRaySeperationDistance;
    private float verticalRaySeperationDistance;
    private int jumpPhysicFrameCount = 0;
    private float jumpW;
    private bool isJumping;
    private bool apexReached;
    private Player player;
    private BoxCollider2D boxCollider;
    private Rigidbody2D rb2d;


    private void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        rb2d = GetComponent<Rigidbody2D>();
        player = GetComponent<Player>();
        CalculateRaySperationDistance();
        jumpW = (Mathf.PI / 2) / (timeToReachJumpApex / 0.02f); // Calculate jumpW based on timeToReachApex
    }

    private void Update()
    {
        CalculateRaycastOrigins();
        CastVerticalRays();
        CastHorizontalRays();
    }

    private void FixedUpdate()
    {
        HandleMove();
        HandleJump();
    }

    private void CastHorizontalRays()
    {
        for (int i = 0; i < horizontalRayCount; i++)
        {
            Vector3 direction = Vector3.left;
            Vector3 newOrigin = new Vector3(raycastOrigins.TopLeft.x, 
                                            raycastOrigins.TopLeft.y - i * horizontalRaySeperationDistance, raycastOrigins.TopLeft.z);
            RaycastHit2D hit = Physics2D.Raycast(newOrigin, direction, rayLength + skinWidth, groundLayerMask);
            isLeftWalled = (hit && hit.normal.x > 0.8f) ? true : false;
            Debug.DrawRay(newOrigin, direction * (rayLength + skinWidth), Color.green);
        }

        for (int i = 0; i < horizontalRayCount; i++)
        {
            Vector3 direction = Vector3.right;
            Vector3 newOrigin = new Vector3(raycastOrigins.TopRight.x,
                                            raycastOrigins.TopRight.y - i * horizontalRaySeperationDistance, raycastOrigins.TopRight.z);
            RaycastHit2D hit = Physics2D.Raycast(newOrigin, direction, rayLength + skinWidth, groundLayerMask);
            isRightWalled = (hit && hit.normal.x < -0.8f) ? true : false;
            Debug.DrawRay(newOrigin, direction * (rayLength + skinWidth), Color.green);
        }
    }
    private void CastVerticalRays()
    {
        //casting downwards
        for (int i = 0; i < verticalRayCount; i++)
        {
            Vector3 direcion = Vector3.down;
            Vector3 newOrigin = new Vector3(raycastOrigins.BottomLeft.x + i * verticalRaySeperationDistance, raycastOrigins.BottomLeft.y, raycastOrigins.BottomLeft.z);
            RaycastHit2D hit = Physics2D.Raycast(newOrigin, direcion, rayLength + skinWidth, groundLayerMask);
            isGrounded = (hit && hit.normal.y > 0.5f) ? true : false;
            Debug.DrawRay(newOrigin, direcion * (rayLength + skinWidth), Color.green);
        }
        //casting upwards
        for (int i = 0; i < verticalRayCount; i++)
        {
            Vector3 direcion = Vector3.up;
            Vector3 newOrigin = new Vector3(raycastOrigins.TopLeft.x + i * verticalRaySeperationDistance, raycastOrigins.TopLeft.y, raycastOrigins.TopLeft.z);
            RaycastHit2D hit = Physics2D.Raycast(newOrigin, direcion, rayLength + skinWidth, groundLayerMask);
            isCeilinged = (hit && hit.normal.y < -0.5f) ? true : false;
            Debug.DrawRay(newOrigin, direcion * (rayLength + skinWidth), Color.green);
        }
    }

    private void CalculateRaycastOrigins()
    {
        raycastOrigins.TopLeft = new Vector3(boxCollider.bounds.min.x + skinWidth, boxCollider.bounds.max.y - skinWidth, 0);
        raycastOrigins.TopRight = new Vector3(boxCollider.bounds.max.x - skinWidth, boxCollider.bounds.max.y - skinWidth, 0);
        raycastOrigins.BottomLeft = new Vector3(boxCollider.bounds.min.x + skinWidth, boxCollider.bounds.min.y + skinWidth, 0);
        raycastOrigins.BottomRight = new Vector3(boxCollider.bounds.max.x - skinWidth, boxCollider.bounds.min.y + skinWidth, 0);
    }
    private void CalculateRaySperationDistance()
    {
        horizontalRaySeperationDistance = (boxCollider.bounds.size.y - skinWidth * 2) / (verticalRayCount - 1);
        verticalRaySeperationDistance = (boxCollider.bounds.size.x - skinWidth * 2) / (horizontalRayCount - 1);
    }

    public bool IsOnGround()
    {
        return colliderStandedOn.Count < 1 ? false : true;
    }

    public bool IsWalledLeft()
    {
        return colliderWalledLeft.Count < 1 ? false : true;
    }

    public bool IsWalledRight()
    {
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

    public void Jump()
    {

        if (!IsOnGround() && !IsWalledRight() && !IsWalledLeft())
        {
            return;
        }//before jump

        int wallJumpDirection = GetWallJumpDirection();

        if (wallJumpDirection == 0)
        {
            rb2d.velocity = new Vector2(rb2d.velocity.x, basicJumpStrength);
            isJumping = true;
        }//normal jump
        else
        {
            rb2d.velocity = new Vector2(rb2d.velocity.x + wallJumpDirection * wallJumpHorizontalStrength, rb2d.velocity.y / 2 + wallJumpVerticalStrength);
            isJumping = true;
        }//wall jump

    }

    public void StopJump()
    {
        isJumping = false;
        apexReached = false;
        jumpPhysicFrameCount = 0;
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
            jumpPhysicFrameCount++;
            float C = Mathf.Cos(jumpW * jumpPhysicFrameCount);
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

        if (other.collider.CompareTag("Ground"))
        {

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
        //Debug.Log(contactPoint.normal.y);
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
