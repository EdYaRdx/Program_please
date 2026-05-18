using System;
using UnityEngine;
using Zenject;

namespace ConfigurationProvider
{
    [CreateAssetMenu(menuName = "ScriptableObject/Configuration/ConfigurationBuilder", fileName = nameof(ConfigurationBuilder))]
    public class ConfigurationBuilder : ScriptableObject, IConfigurationBuilder
    {
        [SerializeField] private ConfigurationProfileBuilderMetadata[] _allProfileBuilders;

        [SerializeField] private ConfigurationProfileBuilder _defaultConfigurationProfileBuilder;

        private DiContainer _addedConfigurations;
        private DiContainer _installedProfileConfigurations;
        private DiContainer _defaultConfigurations;

        private State _currentState;

        private void OnEnable()
        {
            _currentState = State.Start;
        }

        public static ConfigurationBuilder LoadFromResources()
        {
            return Resources.Load<ConfigurationBuilder>(nameof(ConfigurationBuilder));
        }

        public void AddConfiguration(object configuration)
        {
            AssertThatInitialized();

            _addedConfigurations
                .Bind(configuration.GetType())
                .FromInstance(configuration)
                .AsSingle();
        }

        public void AddConfiguration<T>(T configuration)
        {
            AssertThatInitialized();

            _addedConfigurations
                .Bind<T>()
                .FromInstance(configuration)
                .AsSingle();
        }

        public IConfigurationCollection Build()
        {
            AssertThatInitialized();

            _currentState = State.Builded;

            return this;
        }

        public bool TryGet<T>(out T configuration) where T : class
        {
            bool hasBinding = TryGet(typeof(T), out object configurationObject);
            configuration = configurationObject as T;
            return hasBinding;
        }

        public bool TryGet(Type type, out object configuration)
        {
            AssertThatInitializedOrBuilded();

            configuration = _addedConfigurations.TryResolve(type);

            if (configuration == null)
                configuration = _installedProfileConfigurations.TryResolve(type);

            if (configuration == null)
                configuration = _defaultConfigurations.TryResolve(type);

            return configuration != null;
        }

        private void AssertThatInitializedOrBuilded()
        {
            if (_currentState == State.Builded)
                return;

            Initialize();
        }

        private void AssertThatInitialized()
        {
            if (_currentState == State.Builded)
                throw new InvalidOperationException("Cannot use some methods after building of ConfigurationBuilder");

            Initialize();
        }

        private void Initialize()
        {
            if (_currentState == State.Initialized)
                return;

            for (int i = 0; i < _allProfileBuilders.Length; i++)
                if (_allProfileBuilders[i].isActive)
                {
                    _installedProfileConfigurations = new();
                    _allProfileBuilders[i].profileBuilder.Install(_installedProfileConfigurations);
                    break;
                }

            if (_installedProfileConfigurations == null)
                throw new InvalidOperationException("There is not selected ConfigurationProfileBuilder");

            _addedConfigurations = new DiContainer();

            _defaultConfigurations = new DiContainer();
            _defaultConfigurationProfileBuilder?.Install(_defaultConfigurations);

            _currentState = State.Initialized;
        }

        [Serializable]
        public class ConfigurationProfileBuilderMetadata
        {
            public ConfigurationProfileBuilder profileBuilder;
            public bool isActive;
        }

        private enum State
        {
            Start,
            Initialized,
            Builded
        }
    }
}
