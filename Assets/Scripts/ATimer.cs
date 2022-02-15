using System.Timers;

public class ATimer
{
    private float time;
    private ElapsedEventHandler callBack;
    private Timer t;
    public ATimer(float time, ElapsedEventHandler callBack)
    {
        this.time = time;
        this.callBack = callBack;
        t = new Timer(time * 1000);
        t.Elapsed += callBack;
        t.Elapsed += EndTimerAuto;
    }

    public void StartTimer()
    {
        t.Start();
    }

    public void EndTimer()
    {
        t.Stop();
    }

    private void EndTimerAuto(object sender, ElapsedEventArgs e)
    {
        t.Stop();
    }
}
