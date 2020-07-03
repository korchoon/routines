// ----------------------------------------------------------------------------
// Async Reactors framework https://github.com/korchoon/async-reactors
// Copyright (c) 2016-2019 Mikhail Korchun <korchoon@gmail.com>
// ----------------------------------------------------------------------------

using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Mk.Routines
{
    [AsyncMethodBuilder(typeof(RoutineBuilder<>))]
    public sealed class Routine<T>
    {
        internal IScope Scope;
        internal IDisposable Dispose;

        Option<T> _result;
        IScope _awaitersScope;

        internal void SetResult(T res) => _result = res;

        internal Routine()
        {
            Dispose = Sch.Scope.SubScope(out Scope);
            Scope.SubScope(out _awaitersScope);
        }

        [UsedImplicitly]
        public Awaiter<T> GetAwaiter()
        {
            var aw = new Awaiter(_awaitersScope, Dispose.Dispose);
            return new Awaiter<T>(aw, () => _result.GetOrFail());
        }

        public Optional ToOptional() => new Optional(GetAwaiterOptional(), Dispose, Scope);

        Awaiter<Option<T>> GetAwaiterOptional()
        {
            // todo assert awaiterScope is empty
            // todo assert awaiterScope will not be used (no subscribes)
            var aw = new Awaiter(_awaitersScope, Dispose.Dispose);
            return new Awaiter<Option<T>>(aw, () => _result);
        }

        public class Optional : IDisposable
        {
            Awaiter<Option<T>> _aw;
            IDisposable _disposable;
            IScope _scope;

            public Optional(Awaiter<Option<T>> aw, IDisposable disposable, IScope scope)
            {
                _aw = aw;
                _disposable = disposable;
                _scope = scope;
            }

            // todo: should I copy or reuse?
            public Awaiter<Option<T>> GetAwaiter() => _aw;

            public IScope GetScope(IScope scope)
            {
                scope.Subscribe(Dispose);
                return _scope;
            }

            public void Dispose()
            {
                _disposable.Dispose();
            }
        }
    }
}