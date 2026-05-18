using System;
using UnityEngine;
using Zenject;

namespace UnitySaveTool
{
    public class JsonUtilityDataConverter : IGenericDataConverter
    {
        private readonly DiContainer _container;

        public JsonUtilityDataConverter(DiContainer container)
        {
            _container = container;
        }

        public string ConvertFromObject(object obj)
        {
            if (obj is IBeforeConversionCallbackReceiver reciever)
                reciever.OnBeforeConvertation();

            return JsonUtility.ToJson(obj, true);
        }

        public object ConvertToObject(string objectSrting, Type objectType)
        {
            object obj = JsonUtility.FromJson(objectSrting, objectType);

            _container.Inject(obj);

            if (obj is IAfterConversionCallbackReceiver reciever)
                reciever.OnAfterConvertation();

            return obj;
        }

        public T ConvertToObject<T>(string objectSrting) where T : class
        {
            return ConvertToObject(objectSrting, typeof(T)) as T;
        }
    }
}
