using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class BoxColliderRayCastManager : MonoBehaviour
{
    [SerializeField] private bool up, down, left, right;
    [SerializeField] private float rayLength;
    [SerializeField] private int horizontalRayCount;
    [SerializeField] private int verticalRayCount;
    [SerializeField] private float skinWidth = 0.02f;
    [SerializeField] private LayerMask groundLayerMask;
    private BoxCollider2D boxCollider;
    private RaycastOrigins raycastOrigins;
    private BoxColliderRayCastResult result;
    private float horizontalRaySeperationDistance;
    private float verticalRaySeperationDistance;

    private void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
    }

    private void Update()
    {
        CastRays();
    }

    private void CastRays()
    {
        CalculateRaycastOrigins();
        CalculateRaySperationDistance();
        if(down) CastVerticalRays(0);
        if(up) CastVerticalRays(1);
        if(left) CastHorizontalRays(0);
        if(right) CastHorizontalRays(1);
    }

    /// <summary>
    /// Cast rays horizontally to check if the character is touching the left or the right wall
    /// </summary>
    private void CastHorizontalRays(int direction)
    {
        if (direction == 1) result.rightHits = 0;
        if (direction == 0) result.leftHits = 0;

        for (int i = 0; i < horizontalRayCount; i++)
        {
            Vector3 dir = direction == 1 ? Vector3.right : Vector3.left;
            Vector3 origin = direction == 1 ? raycastOrigins.TopRight : raycastOrigins.TopLeft;
            Vector3 newOrigin = new Vector3(origin.x, origin.y - i * horizontalRaySeperationDistance, origin.z);
            RaycastHit2D hit = Physics2D.Raycast(newOrigin, dir, rayLength + skinWidth, groundLayerMask);

            result.rightHits += (hit && hit.normal.x < -0.8f) ? 1 : 0;
            result.leftHits += (hit && hit.normal.x > 0.8f) ? 1 : 0;
            Debug.DrawRay(newOrigin, dir * (rayLength + skinWidth), Color.green);
        }
    }

    /// <summary>
    /// Cast rays vertically to check if the character is touching the ground or the ceiling
    /// </summary>
    private void CastVerticalRays(int direction)
    {
        if (direction == 1) result.upHits = 0;
        if (direction == 0) result.downHits = 0;

        for (int i = 0; i < verticalRayCount; i++)
        {
            Vector3 dir = direction == 1 ? Vector3.up : Vector3.down;
            Vector3 origin = direction == 1 ? raycastOrigins.TopLeft : raycastOrigins.BottomLeft;
            Vector3 newOrigin = new Vector3(origin.x + i * verticalRaySeperationDistance, origin.y, origin.z);
            RaycastHit2D hit = Physics2D.Raycast(newOrigin, dir, rayLength + skinWidth, groundLayerMask);
           
            result.upHits += (hit && hit.normal.y < -0.5f) ? 1 : 0;
            result.downHits += (hit && hit.normal.y > 0.5f) ? 1 : 0;
            Debug.DrawRay(newOrigin, dir * (rayLength + skinWidth), Color.green);
        }
    }

    private void CalculateRaySperationDistance()
    {
        horizontalRaySeperationDistance = (boxCollider.bounds.size.y - skinWidth * 2) / (horizontalRayCount - 1);
        verticalRaySeperationDistance = (boxCollider.bounds.size.x - skinWidth * 2) / (verticalRayCount - 1);
    }

    private void CalculateRaycastOrigins()
    {
        raycastOrigins.TopLeft = new Vector3(boxCollider.bounds.min.x + skinWidth, boxCollider.bounds.max.y - skinWidth, 0);
        raycastOrigins.TopRight = new Vector3(boxCollider.bounds.max.x - skinWidth, boxCollider.bounds.max.y - skinWidth, 0);
        raycastOrigins.BottomLeft = new Vector3(boxCollider.bounds.min.x + skinWidth, boxCollider.bounds.min.y + skinWidth, 0);
        raycastOrigins.BottomRight = new Vector3(boxCollider.bounds.max.x - skinWidth, boxCollider.bounds.min.y + skinWidth, 0);
    }

    public BoxColliderRayCastResult getResult()
    {
        return result;
    }
}
