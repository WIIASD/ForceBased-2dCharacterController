using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Controller2D controller2D;
    public bool JumpPressed {get; private set;}
    public bool JumpReleased {get; private set;}
    public float Horaxis{get; private set;}

    private void Start() {
        controller2D = GetComponent<Controller2D>();
    }
    private void Update()
    {
        updateInput();
    }

    /// <summary>
    /// Update input from player every frame
    /// </summary>
    void updateInput()
    {
        Horaxis = Input.GetAxisRaw("Horizontal");

        if (Input.GetKeyDown(KeyCode.Space))
        {
            controller2D.Jump();
            JumpPressed = true;
            JumpReleased = false;
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            controller2D.StopJump();
            JumpPressed = false;
            JumpReleased = true;
        }
    }

}
