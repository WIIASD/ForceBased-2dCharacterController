using System;

[Serializable]
public struct BoxColliderRayCastResult
{
    public int leftHits, rightHits, upHits, downHits;

    public void clearHorizontal()
    {
        leftHits = rightHits = 0;
    }

    public void clearVertical()
    {
        upHits = downHits = 0;
    }
}
