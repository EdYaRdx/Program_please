using System;

namespace UnityFunctools 
{ 
    public interface IGameLoop
    {
        public event Action<float> UpdateCallback;
        public event Action<float> FixedUpdateCallback;
    }
}