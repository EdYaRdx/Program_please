using Zenject;

namespace ElementaryCase
{
    public class MainProjectInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container
                .Bind<IGlobalMusicController>()
                .To<GlobalMusicController>()
                .AsSingle()
                .NonLazy();

            Container
                .Bind(typeof(ICursorController), typeof(ILateTickable))
                .To<DefaultCursorController>()
                .AsSingle()
                .NonLazy();

            Container
                .Bind<InputSystem_Actions>()
                .AsSingle()
                .OnInstantiated<InputSystem_Actions>((_, system) => system.Enable());
        }
    }
}