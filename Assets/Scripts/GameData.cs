using UnityEngine;

[CreateAssetMenu(fileName = "GameData", menuName = "Scriptable Objects/GameData")]
public class GameData : ScriptableObject
{
    public GameObject[] queueItemPrefabs;
    public AudioClip[] queueItemSpawnSounds;
}
