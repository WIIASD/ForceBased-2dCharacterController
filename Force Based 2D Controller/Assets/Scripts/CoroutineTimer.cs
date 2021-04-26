using System;
using System.Collections;
using UnityEngine;

public class CoroutineTimer
{
    public float time;
    public Action timerCallBack;
    
    public CoroutineTimer(float time, Action timerCallBack)
    {
        this.time = time;
        this.timerCallBack = timerCallBack;
    }

    public void StartTimer(MonoBehaviour monoBehaviour)
    {
        monoBehaviour.StartCoroutine(CallBack());
    }

    private IEnumerator CallBack()
    {
        yield return new WaitForSeconds(time);
        timerCallBack();
    }
   
}
