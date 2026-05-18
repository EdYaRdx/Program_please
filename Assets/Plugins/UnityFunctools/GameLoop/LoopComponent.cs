using System;
using UnityEngine;

namespace UnityFunctools
{
    public class LoopComponent : MonoBehaviour
    {
        public event Action<float> UpdateCallback;
        public event Action<float> FixedUpdateCallback;

        private void Update()
        {
            UpdateCallback?.Invoke(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            FixedUpdateCallback?.Invoke(Time.fixedDeltaTime);
        }
    }
}
