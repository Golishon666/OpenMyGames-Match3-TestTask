using UnityEngine;
using System;

public class SaveService : ISaveService
{
    private const string LevelKey = "CurrentLevelIndex";
    private const string BoardKey = "SavedBoardState";

    [Serializable]
    private class SavedBoard
    {
        public LevelData levelData;
    }

    public void SaveLevel(int levelIndex)
    {
        PlayerPrefs.SetInt(LevelKey, levelIndex);
        PlayerPrefs.Save();
    }

    public void SaveBoard(int width, int height, ElementType[,] elements)
    {
        var levelData = ScriptableObject.CreateInstance<LevelData>();
        levelData.width = width;
        levelData.height = height;
        levelData.elements = new ElementType[width * height];

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                levelData.elements[y * width + x] = elements[x, y];
            }
        }

        var saved = new SavedBoard
        {
            levelData = levelData
        };

        var json = JsonUtility.ToJson(saved.levelData);
        PlayerPrefs.SetString(BoardKey, json);
        PlayerPrefs.Save();
        
        UnityEngine.Object.DestroyImmediate(levelData);
    }

    public int LoadLevelIndex()
    {
        return PlayerPrefs.GetInt(LevelKey, 0);
    }

    public bool HasSavedBoard()
    {
        return PlayerPrefs.HasKey(BoardKey);
    }

    public LevelData LoadBoard()
    {
        var json = PlayerPrefs.GetString(BoardKey);
        var levelData = ScriptableObject.CreateInstance<LevelData>();
        JsonUtility.FromJsonOverwrite(json, levelData);

        return levelData;
    }

    public void ClearBoard()
    {
        PlayerPrefs.DeleteKey(BoardKey);
        PlayerPrefs.Save();
    }
}
