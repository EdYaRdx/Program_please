using System;
using System.Collections.Generic;

namespace UnitySaveTool
{
    public class SaveContextManager : IGlobalSaveContextManager, IGameProgressDataContextManager, ISceneSaveContextManager, IDefaultDataInstanceResolver
    {
        private readonly Dictionary<Type, object> _defaultValues;

        private readonly ISaveContext _globalSaveContext;
        private ISaveContext _gameProgressSaveContext;

        public SaveContextManager(ISaveContext globalSaveContext)
        {
            _globalSaveContext = globalSaveContext;

            _defaultValues = _globalSaveContext.GetChild("DefaultObjects").LoadAll();
        }

        public ISaveContext GetGlobalContext()
        {
            _globalSaveContext.LoadDataToCache();
            return _globalSaveContext;
        }

        public (ISaveContext global, ISaveContext gameProgress) GetGameProgressContext(int saveCellIndex)
        {
            _gameProgressSaveContext = _globalSaveContext.GetChild(saveCellIndex.ToString());
            _gameProgressSaveContext.LoadDataToCache();
            return (_globalSaveContext, _gameProgressSaveContext);
        }

        public (ISaveContext global, ISaveContext gameProgress, ISaveContext scene) GetSceneContext(string sceneName)
        {
            if (_gameProgressSaveContext == null)
                throw new Exception();

            ISaveContext instance = _gameProgressSaveContext.GetChild(sceneName);
            instance.LoadDataToCache();
            return (_globalSaveContext, _gameProgressSaveContext, instance);
        }

        public bool TryGetDefaultDataInstance(Type dataType, out object dataInstacne)
        {
            return _defaultValues.TryGetValue(dataType, out dataInstacne);
        }
    }
}