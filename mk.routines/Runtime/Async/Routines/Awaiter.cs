// ----------------------------------------------------------------------------
// Async Reactors framework https://github.com/korchoon/async-reactors
// Copyright (c) 2016-2019 Mikhail Korchun <korchoon@gmail.com>
// ----------------------------------------------------------------------------

using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Mk.Routines
{
    public class Awaiter : ICriticalNotifyCompletion, IBreakable
    {
        IScope _scope;
        Action _break;
        Action _continuation;
        Action _unsub;

        public void BreakInner()
        {
            InvokeAndFree(ref _break);
        }

        public void Unsubscribe()
        {
            InvokeAndFree(ref _unsub);
        }

        public Awaiter(IScope scope, Action onBreakInner)
        {
            _scope = scope;
            if (_scope.Disposed)
            {
                _unsub = Empty.Action();
                return;
            }

            _break = onBreakInner;
            _scope.Subscribe(OnDispose);
            _unsub = Unsubscribe;

            void Unsubscribe()
            {
                if (!_scope.Disposing)
                    _scope.Unsubscribe(OnDispose);
            }

            void OnDispose()
            {
                if (_continuation != null)
                    InvokeAndFree(ref _continuation);
            }
        }

        [UsedImplicitly] public bool IsCompleted => _scope.Disposed;

        [UsedImplicitly]
        public void GetResult()
        {
        }

        public void OnCompleted(Action continuation)
        {
            if (IsCompleted)
            {
                continuation.Invoke();
                return;
            }

            _continuation += continuation;
        }

        public void UnsafeOnCompleted(Action continuation) => OnCompleted(continuation);

        static void InvokeAndFree(ref Action action)
        {
            var tmp = action;
            action = Empty.Action();
            tmp.Invoke();
        }
    }

    public class Awaiter<T> : ICriticalNotifyCompletion, IBreakable
    {
        readonly Awaiter _aw;
        Func<T> _getResult;

        public Awaiter(Awaiter aw, Func<T> getResult)
        {
            _aw = aw;
            _getResult = getResult;
        }

        [UsedImplicitly]
        public T GetResult() => _getResult.Invoke();

        [UsedImplicitly] public bool IsCompleted => _aw.IsCompleted;

        public void OnCompleted(Action continuation) => _aw.OnCompleted(continuation);
        public void UnsafeOnCompleted(Action continuation) => _aw.UnsafeOnCompleted(continuation);
        public void BreakInner() => _aw.BreakInner();
        public void Unsubscribe() => _aw.Unsubscribe();
    }
}