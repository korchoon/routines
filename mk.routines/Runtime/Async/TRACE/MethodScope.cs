// ----------------------------------------------------------------------------
// Async Reactors framework https://github.com/korchoon/async-reactors
// Copyright (c) 2016-2019 Mikhail Korchun <korchoon@gmail.com>
// ----------------------------------------------------------------------------

using System;

namespace Mk.Routines
{
    public class MethodScope : IDisposable
    {
        public bool Completed { get; private set; }

        void IDisposable.Dispose() => Completed = true;
    }
}