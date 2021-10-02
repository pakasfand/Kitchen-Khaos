using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestartGame : MonoBehaviour
{
    public void RestartShiftIndex()
    {
        GameLoop.currentShiftIndex = 0;
        GameLoop.startFromBeginnig = true;
    }
}
