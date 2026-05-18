using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;
using System;

namespace UnitySaveTool
{
    [CreateAssetMenu(menuName = "Installers/ProjectContext/FileSystemInstaller", fileName = "FileSystemInstaller")]
    public class DefaultFileSystemInstaller : ScriptableObjectInstaller
    {
        [SerializeField]
        private bool _defaultSavePath = true;

        [SerializeField]
        [HideIf(nameof(_defaultSavePath))]
        private string _saveFolderPath;

        public override void InstallBindings()
        {
            Container
                .Bind<IDataConverter>()
                .To<JsonUtilityDataConverter>()
                .AsSingle();

            Container
                .Bind<IFolderMetadataFactory>()
                .To<FolderMetadataFactory>()
                .AsSingle();

            var pathFiderBinding = Container.Bind<IPathFinder>().To<PathFinder>().AsSingle();
            if (_defaultSavePath == false)
            {
                if (_saveFolderPath == null)
                    throw new NullReferenceException();

                pathFiderBinding.WithArguments(_saveFolderPath);
            }
            else
            {
                pathFiderBinding.WithArguments(Application.persistentDataPath + "/UnitySaveTool");
            }

            Container
                .Bind<IFileSystem>()
                .To<FileSystem>()
                .AsSingle()
                .WhenInjectedInto<SaveContext>();
            Container
                .Bind<ISaveContext>()
                .To<SaveContext>()
                .AsSingle()
                .WhenInjectedInto<SaveContextManager>();
            Container
                .Bind(
                    typeof(IGlobalSaveContextManager), 
                    typeof(IGameProgressDataContextManager), 
                    typeof(ISceneSaveContextManager),
                    typeof(IDefaultDataInstanceResolver))
                .To<SaveContextManager>()
                .AsSingle();
        }
    }
}