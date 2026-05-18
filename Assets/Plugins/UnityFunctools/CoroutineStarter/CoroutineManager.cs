using System.Collections;
using UnityEngine;

namespace UnityFunctools
{
    public class CoroutineManager : ICoroutineManager
    {
        private readonly CoroutineProvider _coroutineProvider;

        public CoroutineManager()
        {
            _coroutineProvider = new GameObject("CoroutineProvider").AddComponent<CoroutineProvider>();
            Object.DontDestroyOnLoad(_coroutineProvider);
        }

        public Coroutine Start(IEnumerator routine)
        {
            return _coroutineProvider.StartCoroutine(routine);
        }

        public void Stop(Coroutine coroutine)
        {
            _coroutineProvider.StopCoroutine(coroutine);
        }
    }

    public class CoroutineProvider : MonoBehaviour { }
}