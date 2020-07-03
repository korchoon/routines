// ----------------------------------------------------------------------------
// Async Reactors framework https://github.com/korchoon/async-reactors
// Copyright (c) 2016-2019 Mikhail Korchun <korchoon@gmail.com>
// ----------------------------------------------------------------------------

using System;
using Mk.Debugs;

namespace Mk.Routines
{
    public static class React
    {
        [MustUseReturnValue]
        public static (IPublish pub, ISubscribe sub) PubSub(this IScope scope)
        {
            Asr.IsFalse(scope.Disposing);
            var subject = new Subject(scope);
            return (subject, subject);
        }

        [MustUseReturnValue]
        public static (IPublish<T> pub, ISubscribe<T> sub) PubSub<T>(this IScope scope)
        {
            var subject = new Subject<T>(scope);
            return (subject, subject);
        }

        [MustUseReturnValue]
        public static IDisposable Scope(out IScope scope)
        {
            var subject = new ScopeStack();
            scope = subject;
            return subject;
        }
    }
}