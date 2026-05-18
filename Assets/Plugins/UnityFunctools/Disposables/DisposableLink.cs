using System;

namespace UnityFunctools
{
    public class DisposableLink : IDisposable
    {
        private DisposableObject _disposableObject;

        public DisposableLink(DisposableObject disposableObject)
        {
            _disposableObject = disposableObject;
        }

        public DisposableLink(IDisposable disposable)
        {
            _disposableObject = new DisposableObject(disposable);
        }

        public DisposableLink(Action disposeCallback)
        {
            _disposableObject = new DisposableObject(disposeCallback);
        }

        public void Dispose()
        {
            _disposableObject.Dispose();
        }

        public void Add(DisposableObject newDisposableObject)
        {
            _disposableObject += newDisposableObject;
        }

        public void Add(IDisposable newDisposable)
        {
            _disposableObject += newDisposable;
        }

        public void Add(Action newDisposeCallback)
        {
            _disposableObject += newDisposeCallback;
        }

        public static DisposableLink operator +(DisposableLink obj1, DisposableObject obj2)
        {
            obj1.Add(obj2);
            return obj1;
        }

        public static DisposableLink operator +(DisposableLink obj1, Action obj2)
        {
            obj1.Add(obj2);
            return obj1;
        }

        public static DisposableLink operator +(DisposableLink obj1, IDisposable obj2)
        {
            obj1.Add(obj2);
            return obj1;
        }
    }
}
