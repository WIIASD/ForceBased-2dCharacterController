using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ATimer
{
    private static int PhysicsFramesToMS(int physicsFrames)
    {
        return (int)(((float)physicsFrames * 0.02f) * 1000f);
    }
    public static void StartPhysicFrameTimer(int physicFrames, TimerCallback callBack)
    {
        Timer t = new Timer(callBack);
        t.Change(PhysicsFramesToMS(physicFrames), 0);
    }

    public static void StartTimer(int seconds, TimerCallback callBack)
    {
        Timer t = new Timer(callBack);
        t.Change(seconds * 1000, 0);
    }
}
