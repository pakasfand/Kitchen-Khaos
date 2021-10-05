using UnityEngine;

public class RestartGame : MonoBehaviour
{
    public void RestartShiftIndex()
    {
        GameLoop.currentShiftIndex = 0;
        GameLoop.startFromBeginnig = true;
        DestructibleWall.isDestroyed = false;
        WitchEvent.eventFinished = false;
    }
}
