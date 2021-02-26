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
    [SerializeField] private float normalJumpInitialVelocity = 3f;
    [SerializeField] private float timeToReachJumpApex = 0.2f;
    [SerializeField] private float wallJumpHorizontalInitialVelocity = 4f;
    [SerializeField] private float wallJumpVerticalInitialVelocity = 10f;
    [SerializeField] private float skinWidth = 0.02f;
    [SerializeField] private bool isGrounded;
    [SerializeField] private bool isCeilinged;
    [SerializeField] private bool isLeftWalled;
    [SerializeField] private bool isRightWalled;
    [SerializeField] private bool wasGrounded;
    [SerializeField] private bool wasCeilinged;
    [SerializeField] private bool wasLeftWalled;
    [SerializeField] private bool wasRightWalled;
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
    }

    private void Update()
    {
        CastRays();
    }

    private void FixedUpdate()
    {
        HandleMove();
        HandleJump();
    }

    /// <summary>
    /// Calculate the origins and seperation distances of raycast and cast rays Horizontally and Vertically
    /// </summary>
    private void CastRays()
    {
        CalculateRaycastOrigins();
        CastVerticalRays();
        CastHorizontalRays();
    }

    /// <summary>
    /// Cast rays horizontally to check if the character is touching the left or the right wall
    /// </summary>
    private void CastHorizontalRays()
    {
        int hittedRays = 0;
        wasLeftWalled = isLeftWalled;
        wasRightWalled = isRightWalled;

        //cast leftward
        for (int i = 0; i < horizontalRayCount; i++)
        {
            Vector3 direction = Vector3.left;
            Vector3 newOrigin = new Vector3(raycastOrigins.TopLeft.x, 
                                            raycastOrigins.TopLeft.y - i * horizontalRaySeperationDistance, raycastOrigins.TopLeft.z);
            RaycastHit2D hit = Physics2D.Raycast(newOrigin, direction, rayLength + skinWidth, groundLayerMask);
            hittedRays += (hit && hit.normal.x > 0.8f) ? 1 : 0; //If the ray hit a left wall, +1 to the hittedRays counter
            Debug.DrawRay(newOrigin, direction * (rayLength + skinWidth), Color.green);
        }
        isLeftWalled = hittedRays > 0 ? true : false;
        hittedRays = 0;

        //cast rightward
        for (int i = 0; i < horizontalRayCount; i++)
        {
            Vector3 direction = Vector3.right;
            Vector3 newOrigin = new Vector3(raycastOrigins.TopRight.x,
                                            raycastOrigins.TopRight.y - i * horizontalRaySeperationDistance, raycastOrigins.TopRight.z);
            RaycastHit2D hit = Physics2D.Raycast(newOrigin, direction, rayLength + skinWidth, groundLayerMask);
            hittedRays += (hit && hit.normal.x < -0.8f) ? 1 : 0;
            Debug.DrawRay(newOrigin, direction * (rayLength + skinWidth), Color.green);
        }
        isRightWalled = hittedRays > 0 ? true : false;

        //Call corresponding events
        if (!wasLeftWalled && isLeftWalled)
        {
            OnLeftWallEvent();
        }
        if (!wasRightWalled && isRightWalled)
        {
            OnRightWallEvent();
        }
        if (wasLeftWalled && !isLeftWalled)
        {
            LeftLeftWallEvent();
        }
        if(wasRightWalled && !isRightWalled)
        {
            LeftRightWallEvent();
        }
    }

    /// <summary>
    /// Cast rays vertically to check if the character is touching the ground or the ceiling
    /// </summary>
    private void CastVerticalRays()
    {
        int hittedRays = 0;
        wasGrounded = isGrounded;
        wasCeilinged = isCeilinged;

        //casting downwards
        for (int i = 0; i < verticalRayCount; i++)
        {
            Vector3 direcion = Vector3.down;
            Vector3 newOrigin = new Vector3(raycastOrigins.BottomLeft.x + i * verticalRaySeperationDistance,
                                            raycastOrigins.BottomLeft.y, raycastOrigins.BottomLeft.z);
            RaycastHit2D hit = Physics2D.Raycast(newOrigin, direcion, rayLength + skinWidth, groundLayerMask);
            hittedRays += (hit && hit.normal.y > 0.5f) ? 1 : 0;
            Debug.DrawRay(newOrigin, direcion * (rayLength + skinWidth), Color.green);
        }
        isGrounded = hittedRays > 0 ? true : false;
        hittedRays = 0;

        //casting upwards
        for (int i = 0; i < verticalRayCount; i++)
        {
            Vector3 direcion = Vector3.up;
            Vector3 newOrigin = new Vector3(raycastOrigins.TopLeft.x + i * verticalRaySeperationDistance, 
                                            raycastOrigins.TopLeft.y, raycastOrigins.TopLeft.z);
            RaycastHit2D hit = Physics2D.Raycast(newOrigin, direcion, rayLength + skinWidth, groundLayerMask);
            hittedRays += (hit && hit.normal.y < -0.5f) ? 1 : 0;
            Debug.DrawRay(newOrigin, direcion * (rayLength + skinWidth), Color.green);
        }
        isCeilinged = hittedRays > 0 ? true : false;

        //Call corresponding events
        if (!wasGrounded && isGrounded)
        {
            OnGroundEvent();
        }
        if (!wasCeilinged && isCeilinged)
        {
            OnCeilingEvent();
        }
        if(wasGrounded && !isGrounded)
        {
            LeftGroundEvent();
        }
        if(wasCeilinged && !isCeilinged)
        {
            LeftCeilingEvent();
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

    private void OnGroundEvent()
    {
        Debug.Log("Grounded!!");
    }

    private void LeftGroundEvent()
    {
        Debug.Log("Left Ground!!");
    }

    private void OnCeilingEvent()
    {
        Debug.Log("Ceilinged!!");
    }

    private void LeftCeilingEvent()
    {
        Debug.Log("Left Ceiling!!");
    }

    private void OnLeftWallEvent()
    {
        Debug.Log("LeftWalled!!");
    }

    private void LeftLeftWallEvent()
    {
        Debug.Log("Left Left Wall!!");
    }

    private void OnRightWallEvent()
    {
        Debug.Log("RightWalled!!");
    }

    private void LeftRightWallEvent()
    {
        Debug.Log("Left Right Wall!!");
    }

    /// <summary>
    /// Return the direction of walljumping. 0 means cannot walljump, -1 means to the left, and 1 means to the right
    /// </summary>
    /// <returns></returns>
    public int GetWallJumpDirection()
    {
        if (isGrounded)
        {
            return 0;
        }
        if (isLeftWalled)
        {
            return 1;
        }
        if (isRightWalled)
        {
            return -1;
        }
        return 0;
    }

    /// <summary>
    /// Check the jump condition, if canJump, preform a jump.
    /// </summary>
    public void Jump()
    {

        if (!isGrounded && !isRightWalled && !isLeftWalled)
        {//before jump
            return;
        }

        int wallJumpDirection = GetWallJumpDirection();

        if (wallJumpDirection == 0)
        {//normal jump
            rb2d.velocity = new Vector3(rb2d.velocity.x, normalJumpInitialVelocity, 0);
            isJumping = true;
        }
        else
        {//wall jump
            rb2d.velocity = new Vector3(rb2d.velocity.x + wallJumpDirection * wallJumpHorizontalInitialVelocity,
                                        rb2d.velocity.y / 2 + wallJumpVerticalInitialVelocity, 0);
            isJumping = true;
        }

    }

    /// <summary>
    /// Stop the jumping immediately in the mid-air
    /// </summary>
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
        float horizontalInput = player.horaxis;
        Vector2 force = new Vector2(speedModifier * horizontalInput * rb2d.mass, 0);
        rb2d.AddForce(force);
    }

    /// <summary>
    /// Jump by adding a force every FixedUpdate frames
    /// /// </summary>
    void HandleJump()
    {
        if (isJumping)
        {
            jumpW = (Mathf.PI / 2) / (timeToReachJumpApex / 0.02f); // Calculate jumpW based on timeToReachApex
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
