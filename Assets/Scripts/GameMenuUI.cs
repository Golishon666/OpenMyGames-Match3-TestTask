using UnityEngine;
using Zenject;

public class GameMenuUI : MonoBehaviour
{
    private IBoardService _boardService;

    [Inject]
    public void Construct(IBoardService boardService)
    {
        _boardService = boardService;
    }

    public async void OnRestartClick()
    {
        try
        {
            await _boardService.RestartLevelAsync();
        }
        catch (System.OperationCanceledException)
        {
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
        }
    }

    public async void OnNextClick()
    {
        try
        {
            await _boardService.NextLevelAsync();
        }
        catch (System.OperationCanceledException)
        {
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
        }
    }
}
