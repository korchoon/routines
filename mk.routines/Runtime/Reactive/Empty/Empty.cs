// ----------------------------------------------------------------------------
// Async Reactors framework https://github.com/korchoon/async-reactors
// Copyright (c) 2016-2019 Mikhail Korchun <korchoon@gmail.com>
// ----------------------------------------------------------------------------

using System;

namespace Mk.Routines
{
    public static class Empty
    {
        public static Action Action() => ActionEmpty.Empty;
        public static Action<T> Action<T>() => ActionEmpty<T>.Empty;
        public static Func<T> Func<T>() => FuncEmpty<T>.Empty;
    }
}