public interface ISaveService
{
    void SaveLevel(int levelIndex);
    void SaveBoard(int width, int height, ElementType[,] elements);
    int LoadLevelIndex();
    bool HasSavedBoard();
    LevelData LoadBoard();
    void ClearBoard();
}