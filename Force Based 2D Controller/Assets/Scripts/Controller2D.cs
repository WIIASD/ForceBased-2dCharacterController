using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Learned from https://youtu.be/YDwp5tNCKso for the jumping force calculation part
/// </summary
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Player))]

public class Controller2D : MonoBehaviour
{

    [SerializeField] private bool usingGravity = true;
    [SerializeField] private float gravityScale = 20;
    [SerializeField] private float groundHorizontalDrag = 13f;
    [SerializeField] private float airHorizontalDrag = 13f;
    [SerializeField] private float airVerticalDrag = 13f;
    [SerializeField] private float wallVerticalDrag = 13f;
    [SerializeField] private float speedModifier = 3f;
    [SerializeField] private float jumpStrength = 15f;
    [SerializeField] private float wallJumpHorizontalStrength = 5f;
    [SerializeField] private float wallJumpVerticalStrength = 15f;
    [SerializeField] private float normalJumpInitialVelocity = 3f;
    [SerializeField] private float timeToReachJumpApex = 0.2f;
    [SerializeField] private float wallJumpHorizontalInitialVelocity = 4f;
    [SerializeField] private float wallJumpVerticalInitialVelocity = 10f;
    [SerializeField] private float jumpBufferTime = 0.2f;
    [SerializeField] private float coyoteJumpTime = 0.2f;
    [SerializeField] private float wallSlideSpeedModifier = 2f;
    [SerializeField] private float wallSlideMaxVelocity = 3f;
    [SerializeField] private bool isGrounded;
    [SerializeField] private bool isCeilinged;
    [SerializeField] private bool isLeftWalled;
    [SerializeField] private bool isRightWalled;
    [SerializeField] private bool isNormalJumping;
    [SerializeField] private bool isWallJumping;
    [SerializeField] private bool wasGrounded;
    [SerializeField] private bool wasCeilinged;
    [SerializeField] private bool wasLeftWalled;
    [SerializeField] private bool wasRightWalled;

    private BoxColliderRayCastManager rayCastManager;
    private int jumpPhysicFrameCount = 0;
    private float jumpW;
    private int wallJumpDirection = 0;
    private bool jumpApexReached;
    private bool jumpRegisteredInAir;
    private bool isInCoyoteTime;
    private ATimer jumpBufferTimer;
    private ATimer coyoteJumpTimer;
    private Player player;
    private Rigidbody2D rb2d;

    private void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        player = GetComponent<Player>();
        rayCastManager = GetComponent<BoxColliderRayCastManager>();
        jumpBufferTimer = new ATimer(jumpBufferTime, (a,e) => 
        {
            jumpRegisteredInAir = false;
        });
        coyoteJumpTimer = new ATimer(coyoteJumpTime, (a, e) => 
        { 
            isInCoyoteTime = false;
        });
    }

    private void Update()
    {
        CastRays();
    }

    private void FixedUpdate()
    {
        ApplyGravity();
        ApplyDrag();
        HandleMove();
        HandleJump();
    }

    private void ApplyDrag()
    {
        ApplyGroundDrag();
        ApplyAirDrag();
        ApplyWallDrag();
    }

    private void ApplyGravity()
    {
        if (!usingGravity)
        {
            return;
        }
        ApplyInAirGravity();
        ApplyWallSildeGravity();
    }

    /// <summary>
    /// Gravity is calculated seperately while the player is in the mid air on lying on a wall
    /// </summary>
    private void ApplyInAirGravity()
    {
        if (!isGrounded && !isLeftWalled && !isRightWalled)
        {
            Vector2 g = new Vector2(0, rb2d.mass * -9.81f * gravityScale);
            rb2d.AddForce(g);
        }
    }

    private void ApplyWallSildeGravity()
    {
        if (isGrounded)
        {
            return;
        }
        if (isLeftWalled || isRightWalled)
        {
            if (rb2d.velocity.y > -wallSlideMaxVelocity)
            {
                //Vector2 force = new Vector2(0, -wallSlideSpeedModifier);
                //rb2d.AddForce(force, ForceMode2D.Force);
                rb2d.velocity += new Vector2(0, -wallSlideSpeedModifier);
            }
            if (Mathf.Abs(rb2d.velocity.y) < 0.02f)
            {
                rb2d.velocity = new Vector2(rb2d.velocity.x, 0);
            }
        }
    }

    private void ApplyAirDrag()
    {
        if (!isGrounded && !isLeftWalled && !isRightWalled)
        {
            Vector2 force = new Vector2(-airHorizontalDrag * rb2d.velocity.x, -airVerticalDrag * rb2d.velocity.y);
            rb2d.AddForce(force);
        }
    }

    private void ApplyGroundDrag()
    {
        if (isGrounded)
        {
            Vector2 force = new Vector2(-groundHorizontalDrag * rb2d.velocity.x, 0);
            rb2d.AddForce(force);
        }
    }

    private void ApplyWallDrag()
    {
        if (isLeftWalled || isRightWalled)
        {
            Vector2 force = new Vector2(0, -wallVerticalDrag * rb2d.velocity.y);
            rb2d.AddForce(force);
        }
    }

    /// <summary>
    /// Calculate the origins and seperation distances of raycast and cast rays Horizontally and Vertically
    /// </summary>
    private void CastRays()
    {
        BoxColliderRayCastResult result = rayCastManager.getResult();
        wasLeftWalled = isLeftWalled;
        wasRightWalled = isRightWalled;
        wasCeilinged = isCeilinged;
        wasGrounded = isGrounded;
        isLeftWalled = result.leftHits > 0 ? true : false;
        isRightWalled = result.rightHits > 0 ? true : false;
        isCeilinged = result.upHits > 0 ? true : false;
        isGrounded = result.downHits > 0 ? true : false;
        CallOnLeftEventsHorizontal();
        CallOnLeftEventsVertical();
    }

    /// <summary>
    /// Return the direction of walljumping. 0 means cannot walljump, -1 means to the left, and 1 means to the right
    /// </summary>
    /// <returns></returns>
    public int GetCurrentWallJumpDirection()
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
        if (!isGrounded && !isRightWalled && !isLeftWalled && !isInCoyoteTime)
        {//before jump
            jumpRegisteredInAir = true;
            jumpBufferTimer.StartTimer();
            return;
        }

        int currentWallJumpDirection = GetCurrentWallJumpDirection();

        if (currentWallJumpDirection == 0)
        {//normal jump
            rb2d.velocity = new Vector3(rb2d.velocity.x, normalJumpInitialVelocity, 0);
            isNormalJumping = true;
        }
        else
        {//wall jump
            rb2d.velocity = new Vector3(rb2d.velocity.x + currentWallJumpDirection * wallJumpHorizontalInitialVelocity,
                                        rb2d.velocity.y / 2 + wallJumpVerticalInitialVelocity, 0);
            isWallJumping = true;
            wallJumpDirection = currentWallJumpDirection;
        }

    }

    /// <summary>
    /// Stop the jumping immediately in the mid-air
    /// </summary>
    public void StopJump()
    {
        wallJumpDirection = 0;
        isNormalJumping = false;
        isWallJumping = false;
        jumpApexReached = false;
        jumpPhysicFrameCount = 0;
    }

    /// <summary>
    /// move the player by adding a acceleration every FixedUpdate frames
    /// </summary>
    private void HandleMove()
    {
        float horizontalInput = player.Horaxis;
        if (Mathf.Abs(horizontalInput) > 0)
        {
            //Vector2 force = new Vector2(speedModifier * horizontalInput * rb2d.mass, 0);
            //rb2d.AddForce(force);
            rb2d.velocity += new Vector2(horizontalInput * speedModifier, 0);
        }
    }

    /// <summary>
    /// Jump by adding a acceleration every FixedUpdate frames
    /// /// </summary>
    private void HandleJump()
    {
        jumpW = (Mathf.PI / 2) / (timeToReachJumpApex / 0.02f); // Calculate jumpW based on timeToReachApex
        if (isNormalJumping)
        {
            //this is an counter that keep track of the time in air in every FixedUpdate frames (0.02s)
            //will be reset after landing
            jumpPhysicFrameCount++;
            float C = Mathf.Cos(jumpW * jumpPhysicFrameCount);
            if (C < 0f)
            {
                jumpApexReached = true;
                StopJump();
            }
            else
            {
                rb2d.velocity += new Vector2(0f, C * jumpStrength);
            }
        }
        if (isWallJumping)
        {
            jumpPhysicFrameCount++;
            float C = Mathf.Cos(jumpW * jumpPhysicFrameCount);
            if (C < 0f)
            {
                jumpApexReached = true;
                StopJump();
            }
            else
            {
                rb2d.velocity += new Vector2(C * wallJumpHorizontalStrength * wallJumpDirection, C * wallJumpVerticalStrength);
            }
        }
    }

    /// <summary>
    /// Call one of the method in the ON/LEFT region when the player on/left ground/ceiling
    /// </summary>
    private void CallOnLeftEventsVertical()
    {
        //On events
        if (!wasGrounded && isGrounded)
        {
            OnGroundEvent();
        }
        if (!wasCeilinged && isCeilinged)
        {
            OnCeilingEvent();
        }
        //Left events
        if (wasGrounded && !isGrounded)
        {
            LeftGroundEvent();
        }
        if (wasCeilinged && !isCeilinged)
        {
            LeftCeilingEvent();
        }
    }

    /// <summary>
    /// Call one of the method in the ON/LEFT region when the player on/left left wall/right wall
    /// </summary>
    private void CallOnLeftEventsHorizontal()
    {
        //On events
        if (!wasLeftWalled && isLeftWalled)
        {
            OnLeftWallEvent();
        }
        if (!wasRightWalled && isRightWalled)
        {
            OnRightWallEvent();
        }
        //Left events
        if (wasLeftWalled && !isLeftWalled)
        {
            LeftLeftWallEvent();
        }
        if (wasRightWalled && !isRightWalled)
        {
            LeftRightWallEvent();
        }
    }

    #region ON/LEFT: Methods that are going to be called whenever the player: (Touches/left) (Ground/Ceiling/Left Wall/Right Wall)

    private void OnGroundEvent()
    {
        Debug.Log("Grounded!!");
        onJumpableSurface();
    }

    private void LeftGroundEvent()
    {
        Debug.Log("Left Ground!!");
        if (!isNormalJumping)
        {
            isInCoyoteTime = true;
            coyoteJumpTimer.StartTimer();
        }
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
        onJumpableSurface();
    }

    private void LeftLeftWallEvent()
    {
        Debug.Log("Left Left Wall!!");
    }

    private void OnRightWallEvent()
    {
        Debug.Log("RightWalled!!");
        onJumpableSurface();
    }

    private void LeftRightWallEvent()
    {
        Debug.Log("Left Right Wall!!");
    }

    private void onJumpableSurface()
    {
        if (jumpRegisteredInAir)
        {
            Jump();
            jumpRegisteredInAir = false;
        }
    }

    #endregion

    /// <summary>
    /// NOT USED ANYMORE!!
    /// The following 3 methods handle collision to ground with different normal so that the player knows what kind of wall(s)/ground(s) is colliding.
    /// I used arrays to store different gameobjects that are colliding with the player
    /// </summary>
    /// <param name="other"></param>
    //private void OnCollisionEnter2D(Collision2D other)
    //{
    //    ContactPoint2D contactPoint = other.GetContact(0);

    //    if (other.collider.CompareTag("Ground"))
    //    {

    //        if (contactPoint.normal.y > 0.5f) //ground
    //        {
    //            //contactPoint.collider.GetComponent<SpriteRenderer>().color = Color.red;
    //            colliderStandedOn.Add(contactPoint.collider);
    //        }

    //        if (contactPoint.normal.x > 0.8f)//left wall
    //        {
    //            //contactPoint.collider.GetComponent<SpriteRenderer>().color = Color.green;
    //            colliderWalledLeft.Add(contactPoint.collider);
    //        }

    //        if (contactPoint.normal.x < -0.8f)//right wall
    //        {
    //            //contactPoint.collider.GetComponent<SpriteRenderer>().color = Color.blue;
    //            colliderWalledRight.Add(contactPoint.collider);
    //        }
    //    }

    //}

    //private void OnCollisionExit2D(Collision2D other)
    //{

    //    if (colliderStandedOn.Count > 0)
    //    {
    //        if (colliderStandedOn.Contains(other.collider))
    //        {
    //            //other.collider.GetComponent<SpriteRenderer>().color = Color.white;
    //            colliderStandedOn.Remove(other.collider);
    //        }
    //    }

    //    if (colliderWalledLeft.Count > 0)
    //    {
    //        if (colliderWalledLeft.Contains(other.collider))
    //        {
    //            //other.collider.GetComponent<SpriteRenderer>().color = Color.white;
    //            colliderWalledLeft.Remove(other.collider);
    //        }
    //    }

    //    if (colliderWalledRight.Count > 0)
    //    {
    //        if (colliderWalledRight.Contains(other.collider))
    //        {
    //            //other.collider.GetComponent<SpriteRenderer>().color = Color.white;
    //            colliderWalledRight.Remove(other.collider);
    //        }
    //    }

    //}

    ///// <summary>
    ///// To double check if there are unexpected colliders that the player is not colliding with in the collider arrays 
    ///// </summary>
    ///// <param name="other"></param>
    //private void OnCollisionStay2D(Collision2D other)
    //{
    //    ContactPoint2D contactPoint = other.GetContact(0);
    //    //Debug.Log(contactPoint.normal.y);
    //    if (Mathf.Abs(contactPoint.normal.x) < 0.8 &&
    //        (colliderWalledLeft.Contains(other.collider) || colliderWalledRight.Contains(other.collider)))
    //    {
    //        colliderWalledLeft.Remove(other.collider);
    //        colliderWalledRight.Remove(other.collider);
    //        if (contactPoint.normal.y > 0.5)
    //        {
    //            colliderStandedOn.Add(other.collider);
    //            //contactPoint.collider.GetComponent<SpriteRenderer>().color = Color.red;
    //        }
    //    }
    //}


}
