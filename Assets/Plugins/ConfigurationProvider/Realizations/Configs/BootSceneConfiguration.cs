using UnityEngine;
using Zenject;

namespace ConfigurationProvider
{
    [CreateAssetMenu(menuName = "ScriptableObject/Configuration/Object/BootSceneConfiguration", fileName = nameof(BootSceneConfiguration))]
    public class BootSceneConfiguration : ConfigurationObject, IBootScene
    {
        public string Name => _name;

        [SerializeField] private string _name;

        public override void Install(DiContainer container)
        {
            container
                .Bind<IBootScene>()
                .FromInstance(this)
                .AsSingle();
        }
    }
}