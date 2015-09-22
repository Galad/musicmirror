using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicMirror.Tests.Autofixture
{
    public sealed class TestObservableWrapper<T> : IObservable<T>
    {
        private IObservable<T> _observable;

        public TestObservableWrapper(IObservable<T> observable)
        {
            _observable = observable;
        }

        public void SetObservable(IObservable<T> observable)
        {
            _observable = observable;
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            return _observable.Subscribe(observer);
        }
    }
}
