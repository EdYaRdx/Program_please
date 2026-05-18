using System.Collections;
using UnityEngine;

namespace UnityFunctools
{
    public interface ICoroutineManager
    {
        Coroutine Start(IEnumerator coroutine);
        void Stop(Coroutine coroutine);
    }
}
