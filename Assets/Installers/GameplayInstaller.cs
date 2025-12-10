using Pools;
using UnityEngine;
using Zenject;

namespace Installers {
    public class GameplayInstaller : MonoInstaller {
        [SerializeField] private int _boardWidth = 7;
        [SerializeField] private int _boardHeight = 7;

        [SerializeField] private Camera _camera;
        [SerializeField] private InputManager _inputManager;
        [SerializeField] private GameplayScreen _gameplayScreen;

        [SerializeField] private GemRepository _gemRepository;
        [SerializeField] private BoardViewSettings _boardViewSettings;
        [SerializeField] private SpawnableGameObject _gemBackgroundPrefab;

        public override void InstallBindings() {
            Container.BindInterfacesAndSelfTo<MatchCheckStrategy>().AsSingle();
            Container.BindInterfacesAndSelfTo<BoardInitializeStrategy>().AsSingle();
            Container.BindInterfacesAndSelfTo<BoardRefillStrategy>().AsSingle();

            Container.BindInterfacesAndSelfTo<GemPool>().AsSingle();
            Container.BindInterfacesAndSelfTo<GameObjectPool>().AsSingle()
                .WithArguments(_gemBackgroundPrefab, _boardWidth * _boardHeight, 10);

            Container.Bind<Board>().AsSingle().WithArguments(_boardWidth, _boardHeight);
            Container.Bind<BoardView>().AsSingle();
            Container.Bind<GemAbilityProvider>().AsSingle();

            Container.BindInstance(_camera).AsSingle();
            
            Container.BindInstance(_inputManager).AsSingle();
            Container.BindInstance(_gemRepository).AsSingle();
            Container.BindInstance(_boardViewSettings).AsSingle();
            Container.BindInstance(_gameplayScreen).AsSingle();

            Container.BindInterfacesAndSelfTo<GameController>().AsSingle().NonLazy();
        }
    }
}