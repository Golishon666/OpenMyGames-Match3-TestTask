public class LevelService : ILevelService
{
    private readonly LevelsConfig _levelsConfig;
    private readonly ISaveService _saveService;
    private int _currentLevelIndex;

    public int CurrentLevelIndex => _currentLevelIndex;

    public LevelService(LevelsConfig levelsConfig, ISaveService saveService)
    {
        _levelsConfig = levelsConfig;
        _saveService = saveService;
    }

    public LevelData LoadInitialLevel()
    {
        _currentLevelIndex = _saveService.LoadLevelIndex();
        return GetLevelByIndex(_currentLevelIndex);
    }

    public LevelData GetNextLevel()
    {
        _currentLevelIndex++;
        _saveService.SaveLevel(_currentLevelIndex);
        _saveService.ClearBoard();
        return GetLevelByIndex(_currentLevelIndex);
    }

    public LevelData GetCurrentLevel()
    {
        return GetLevelByIndex(_currentLevelIndex);
    }

    public void RestartLevel()
    {
        _saveService.ClearBoard();
    }

    private LevelData GetLevelByIndex(int index)
    {
        return _levelsConfig.levels[index % _levelsConfig.levels.Length];
    }
}
