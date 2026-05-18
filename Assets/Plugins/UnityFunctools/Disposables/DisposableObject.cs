using System;

namespace UnityFunctools
{
    public struct DisposableObject : IDisposable
    {
        private bool _isDisposed;

        private Action _disposeCallback;

        public DisposableObject(Action disposeCallback)
        {
            _disposeCallback = disposeCallback;

            _isDisposed = false;
        }

        public DisposableObject(IDisposable disposable)
        {
            _disposeCallback = disposable.Dispose;

            _isDisposed = false;
        }

        public void Dispose()
        {
            if (_isDisposed)
                throw new Exception("Object is already disposed");

            _disposeCallback?.Invoke();

            _isDisposed = true;
        }

        public static DisposableObject operator +(DisposableObject disposableObj1, DisposableObject disposableObj2)
        {
            return new DisposableObject(() => 
            {
                disposableObj1.Dispose();
                disposableObj2.Dispose();
            });
        }

        public static DisposableObject operator +(DisposableObject disposableObj, Action callback)
        {
            return disposableObj + new DisposableObject(callback);
        }

        public static DisposableObject operator +(DisposableObject disposableObj, IDisposable disposable)
        {
            return disposableObj + new DisposableObject(disposable);
        }
    }
}