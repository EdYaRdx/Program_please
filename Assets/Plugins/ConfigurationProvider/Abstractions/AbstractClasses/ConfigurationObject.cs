using UnityEngine;
using Zenject;

namespace ConfigurationProvider
{
    public abstract class ConfigurationObject : ScriptableObject
    {
        public abstract void Install(DiContainer container);
    }
}