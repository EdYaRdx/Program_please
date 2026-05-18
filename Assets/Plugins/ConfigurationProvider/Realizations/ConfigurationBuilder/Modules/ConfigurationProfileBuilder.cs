using UnityEngine;
using Zenject;

namespace ConfigurationProvider
{
    [CreateAssetMenu(menuName = "ScriptableObject/Configuration/ConfigurationProfileBuilder", fileName = nameof(ConfigurationProfileBuilder))]
    public class ConfigurationProfileBuilder : ScriptableObject
    {
        [SerializeField] private ConfigurationObject[] _configurations;

        public void Install(DiContainer container)
        {
            foreach (ConfigurationObject configuration in _configurations)
                configuration.Install(container);
        }
    }
}