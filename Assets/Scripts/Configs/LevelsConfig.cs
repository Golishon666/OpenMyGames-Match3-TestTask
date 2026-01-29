using UnityEngine;

[CreateAssetMenu(fileName = "LevelsConfig", menuName = "GameConfigs/LevelsConfig")]
public class LevelsConfig : ScriptableObject
{
    public LevelData[] levels;
}