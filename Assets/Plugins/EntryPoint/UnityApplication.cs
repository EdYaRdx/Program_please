using ConfigurationProvider;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace EntryPoint
{
    public class UnityApplication
    {
        private event Action<DiContainer> _installCallback;
        private IConfigurationCollection _configurationCollection;

        internal UnityApplication(Action<DiContainer> installCallback, IConfigurationCollection configurationCollection)
        {
            _installCallback = installCallback;
            _configurationCollection = configurationCollection;
        }

        public void Run()
        {
            ProjectContext.PreInstall += () =>
            {
                ProjectContext.Instance.Container
                    .Bind<IConfigurationCollection>()
                    .FromInstance(_configurationCollection)
                    .AsSingle();

                _installCallback?.Invoke(ProjectContext.Instance.Container);
            };

            if (_configurationCollection.TryGet(out IBootScene bootScene) == false)
                throw new MissingConfigurationException(typeof(IBootScene));

            if (_configurationCollection.TryGet(out IStartScene startScene) == false)
                throw new MissingConfigurationException(typeof(IStartScene));

            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.LoadScene(bootScene.Name);

            void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;

                foreach (GameObject go in scene.GetRootGameObjects())
                {
                    if (go.TryGetComponent(out SceneContext sceneContext))
                    {
                        sceneContext.Run();
                        break;
                    }
                }

                SceneManager.LoadScene(startScene.Name);
            }
        }
    }
}
