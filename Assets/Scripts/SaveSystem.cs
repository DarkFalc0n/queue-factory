using UnityEngine;

public static class SaveSystem
{
    public static void SaveData(PlayerData playerData)
    {
        string json = JsonUtility.ToJson(playerData);
        PlayerPrefs.SetString("playerData", json);
        PlayerPrefs.Save();
    }

    public static PlayerData LoadData()
    {
        string json = PlayerPrefs.GetString("playerData");
        if (string.IsNullOrEmpty(json))
        {
            return new PlayerData();
        }
        return JsonUtility.FromJson<PlayerData>(json);
    }
}
