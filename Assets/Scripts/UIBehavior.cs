using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIBehavior : MonoBehaviour
{

    public TMP_Text Player1StatusText;
    public TMP_Text CPUStatusText;
    public TMP_Text Player2CPUText;

    public void RefreshUI(Snake snakePlayer, Snake snakeEnemy)
    {
        if (GameResultInfo.IsTwoPlayerMode)
        {
            Player2CPUText.text = "Player 2";
        }
        else
        {
            Player2CPUText.text = "CPU";
        }
        
        switch (snakePlayer.currentSpeed)
        {
            case Snake.SpeedState.Normal:
                Player1StatusText.text = "Normal";
                break;

            case Snake.SpeedState.Fast:
                Player1StatusText.text = "Fast";
                break;

            case Snake.SpeedState.Slow:
                Player1StatusText.text = "Slow";
                break;
        }

        switch(snakeEnemy.currentSpeed)
        {
            case Snake.SpeedState.Normal:
                CPUStatusText.text = "Normal";
            break;

            case Snake.SpeedState.Fast:
                CPUStatusText.text = "Fast";
            break;

            case Snake.SpeedState.Slow:
                CPUStatusText.text = "Slow";
            break;
        }
    }
}
