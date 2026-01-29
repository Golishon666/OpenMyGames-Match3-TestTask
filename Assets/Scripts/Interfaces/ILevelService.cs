public interface ILevelService
{
    int CurrentLevelIndex { get; }
    LevelData LoadInitialLevel();
    LevelData GetNextLevel();
    LevelData GetCurrentLevel();
    void RestartLevel();
}
