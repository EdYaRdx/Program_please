using UnityEngine;
using Zenject;

namespace ConfigurationProvider
{
    [CreateAssetMenu(menuName = "ScriptableObject/Configuration/Object/StartSceneConfiguration", fileName = nameof(StartSceneConfiguration))]
    public class StartSceneConfiguration : ConfigurationObject, IStartScene
    {
        public string Name => _name;

        [SerializeField] private string _name;

        public override void Install(DiContainer container)
        {
            container
                .Bind<IStartScene>()
                .FromInstance(this)
                .AsSingle();
        }
    }
}
