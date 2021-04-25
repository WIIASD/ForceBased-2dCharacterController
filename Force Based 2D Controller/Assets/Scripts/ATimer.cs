using System;
using System.Timers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ATimer
{
    private float time;
    private Action toDo;
    private ElapsedEventHandler callBack;
    private Timer t;
    public ATimer(float time, ElapsedEventHandler callBack)
    {
        this.time = time;
        this.callBack = callBack;
        t = new Timer(time * 1000);
        t.Elapsed += callBack;
        t.Elapsed += EndTimer;
    }

    public void StartTimer()
    {
        t.Start();
    }

    public void EndTimer(object sender, ElapsedEventArgs e)
    {
        t.Stop();
    }
}
