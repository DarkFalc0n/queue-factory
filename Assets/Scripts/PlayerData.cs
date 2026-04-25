using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public int unlockedLevel;
    public bool hasSeenTutorial;
    public bool hasSeenCredits;

    public PlayerData()
    {
        unlockedLevel = 1;
        hasSeenTutorial = false;
    }
}
