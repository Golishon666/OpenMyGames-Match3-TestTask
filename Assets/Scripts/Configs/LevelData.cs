using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "GameConfigs/LevelData")]
public class LevelData : ScriptableObject
{
    public int width;
    public int height;
    public ElementType[] elements;

    public ElementType GetElement(int x, int y)
    {
        return elements[y * width + x];
    }
}