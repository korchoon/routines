// ----------------------------------------------------------------------------
// Async Reactors framework https://github.com/korchoon/async-reactors
// Copyright (c) 2016-2019 Mikhail Korchun <korchoon@gmail.com>
// ----------------------------------------------------------------------------

using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Mk.Routines
{
    [AsyncMethodBuilder(typeof(RoutineBuilder))]
    public sealed class Routine : IDisposable
    {
        internal IScope Scope;
        internal IDisposable _Dispose;
        IScope _awaitersScope;

        public IScope GetScope(IScope scope)
        {
            scope.Subscribe(Dispose);
            return Scope;
        }

        internal Routine()
        {
            _Dispose = Sch.Scope.SubScope(out Scope);
            Scope.SubScope(out _awaitersScope);
        }

        [UsedImplicitly]
        public Awaiter GetAwaiter()
        {
            var res = new Awaiter(_awaitersScope, Dispose);
            return res;
        }

        public void Dispose()
        {
            _Dispose.Dispose();
        }

        #region Static API

        public static SelfScope SelfScope() => new SelfScope();
        public static SelfDispose SelfDispose() => new SelfDispose();

        #endregion
    }
}