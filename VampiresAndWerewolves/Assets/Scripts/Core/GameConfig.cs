using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "Vampires/GameConfig")]
public class GameConfig : ScriptableObject
{
    [Header("Server")]
    public string serverUrl = "http://localhost:3000";
    public bool offlineMode = true;
}

