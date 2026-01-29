using System.Threading;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

public class GameInstaller : MonoInstaller
{
    [SerializeField] private LevelsConfig _levelsConfig;
    [SerializeField] private ElementViewFactoryConfig _elementFactoryConfig;
    [SerializeField] private MainGameConfig _mainGameConfig;

    public override void InstallBindings()
    {
        Container.Bind<CancellationToken>().FromInstance(destroyCancellationToken).AsSingle();
        Container.Bind<LevelsConfig>().FromInstance(_levelsConfig).AsSingle();
        Container.Bind<MainGameConfig>().FromInstance(_mainGameConfig).AsSingle();
        Container.Bind<ISaveService>().To<SaveService>().AsSingle();
        Container.Bind<IMatchFinder>().To<MatchFinder>().AsSingle();
        Container.Bind<IGravityService>().To<GravityService>().AsSingle();
        Container.Bind<ILevelService>().To<LevelService>().AsSingle();
        Container.Bind<BoardView>().FromComponentInHierarchy().AsSingle();
        Container.Bind<GameMenuUI>().FromComponentInHierarchy().AsSingle();
        Container.BindInterfacesAndSelfTo<SwipeController>().AsSingle().Lazy();
        Container.Bind<SwipeInput>().FromComponentInHierarchy().AsSingle().NonLazy();
        Container.Bind<IBoardService>().To<BoardService>().AsSingle();
        Container.Bind<ElementViewFactory>().AsSingle()
            .WithArguments(_elementFactoryConfig);
    }
}