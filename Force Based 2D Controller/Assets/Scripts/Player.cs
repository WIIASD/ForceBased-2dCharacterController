using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Controller2D controller2D;
    public bool jumpPressed {get; private set;}
    public bool jumpReleased {get; private set;}
    public float horaxis{get; private set;}

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
        horaxis = Input.GetAxisRaw("Horizontal");

        if (Input.GetKeyDown(KeyCode.Space))
        {
            controller2D.Jump();
            jumpPressed = true;
            jumpReleased = false;
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            controller2D.StopJump();
            jumpPressed = false;
            jumpReleased = true;
        }
    }

}
