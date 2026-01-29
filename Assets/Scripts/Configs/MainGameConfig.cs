using UnityEngine;

[CreateAssetMenu(fileName = "MainGameConfig", menuName = "GameConfigs/MainGameConfig")]
public class MainGameConfig : ScriptableObject
{
    public Vector2 padding = new Vector2(0f, 6f);
    public float horizontalMargin = 1f;
    public Vector2 cellSize = new Vector2(2f, 2f);
    public float defaultScale = 0.8f;
    public float moveElementDuration = 0.2f;
}
