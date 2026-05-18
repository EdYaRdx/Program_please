using ConfigurationProvider;
using System;
using UnityFunctools;
using Zenject;

namespace EntryPoint
{
    public class UnityApplicationBuilder
    {
        public IConfigurationCollection Configurations => _configurationBuilder;

        private Action<DiContainer> _installCallback;

        private IConfigurationBuilder _configurationBuilder;

        private bool _isBuilded;

        public UnityApplicationBuilder(IConfigurationBuilder configurationBuilder)
        {
            _configurationBuilder = configurationBuilder;

            _installCallback = (_) => { };

            _isBuilded = false;
        }

        public UnityApplicationBuilder() : this(ConfigurationBuilder.LoadFromResources()) { }

        public void AddConfiguration(object configuration)
        {
            if (_isBuilded)
                throw new BuilderAlreadyBuildedException(nameof(UnityApplicationBuilder));

            _configurationBuilder.AddConfiguration(configuration);
        }

        public void AddConfiguration<T>(T configuration)
        {
            if (_isBuilded)
                throw new BuilderAlreadyBuildedException(nameof(UnityApplicationBuilder));

            _configurationBuilder.AddConfiguration<T>(configuration);
        }

        public IDisposable AddInstaller(Action<DiContainer> installCallback)
        {
            if (_isBuilded)
                throw new BuilderAlreadyBuildedException(nameof(UnityApplicationBuilder));

            _installCallback += installCallback;

            return new DisposableObject(() => _installCallback -= installCallback);
        }

        public UnityApplication Build()
        {
            _isBuilded = true;

            return new UnityApplication(_installCallback, _configurationBuilder.Build());
        }
    }
}