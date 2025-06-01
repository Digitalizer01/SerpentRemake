using System;
using System.IO;
using UnityEngine;

public static class GameResultInfo
{
    public static bool PlayerWon;
    public static int Time;
    public static bool IsTwoPlayerMode;

    private static string logFilePath = Path.Combine(Application.persistentDataPath, "GameResults.txt");

    public static void LogGameResult(bool playerWon)
    {
        string result = playerWon ? "Player1: Win" : "Player1: Lose";
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string gameTime = Time.ToString();

        string logLine = $"{result};{timestamp};{gameTime}";

        try
        {
            File.AppendAllText(logFilePath, logLine + Environment.NewLine);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to write game log: " + e.Message);
        }
    }
}
