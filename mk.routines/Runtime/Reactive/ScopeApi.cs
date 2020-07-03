// ----------------------------------------------------------------------------
// Async Reactors framework https://github.com/korchoon/async-reactors
// Copyright (c) 2016-2019 Mikhail Korchun <korchoon@gmail.com>
// ----------------------------------------------------------------------------

using System;
using System.Runtime.InteropServices;

namespace Mk.Routines
{
    public static class ScopeApi
    {
        public static ISubscribe<(Option<T1>, Option<T2>, Option<T3>)> BranchOf<T1, T2, T3>(this IScope scope, ISubscribe<T1> s1, ISubscribe<T2> s2, ISubscribe<T3> s3)
        {
            var (pub, sub) = scope.PubSub<(Option<T1>, Option<T2>, Option<T3>)>();
            s1.Subscribe(msg => pub.Publish((msg, default, default)), scope);
            s2.Subscribe(msg => pub.Publish((default, msg, default)), scope);
            s3.Subscribe(msg => pub.Publish((default, default, msg)), scope);

            return sub;
        }

        public static ISubscribe<(bool, Option<T2>, Option<T3>)> BranchOf<T2, T3>(this IScope scope, ISubscribe s1, ISubscribe<T2> s2, ISubscribe<T3> s3)
        {
            var (pub, sub) = scope.PubSub<(bool, Option<T2>, Option<T3>)>();

            s1.Subscribe(() => { pub.Publish((true, default, default)); }, scope);
            s2.Subscribe(msg => { pub.Publish((default, msg, default)); }, scope);
            s3.Subscribe(msg => { pub.Publish((default, default, msg)); }, scope);

            return sub;
        }

        public static ISubscribe<(bool, Option<T2>)> BranchOf<T2>(this IScope scope, ISubscribe s1, ISubscribe<T2> s2)
        {
            var (pub, sub) = scope.PubSub<(bool, Option<T2>)>();

            s1.Subscribe(() => { pub.Publish((true, default)); }, scope);
            s2.Subscribe(msg => { pub.Publish((default, msg)); }, scope);

            return sub;
        }


        public static ISubscribe<(Option<T1>, Option<T2>)> BranchOf<T1, T2>(this IScope scope, ISubscribe<T1> s1, ISubscribe<T2> s2)
        {
            var (pub, sub) = scope.PubSub<(Option<T1>, Option<T2>)>();

            s1.Subscribe(msg => pub.Publish((msg, default)), scope);
            s2.Subscribe(msg => pub.Publish((default, msg)), scope);

            return sub;
        }

        public static ISubscribe<(bool, bool, Option<T>)> BranchOf<T>(this IScope scope, ISubscribe s1, ISubscribe s2, ISubscribe<T> s3)
        {
            var subject = new Subject<(bool, bool, Option<T>)>(scope);
            var (pub, sub) = ((IPublish<(bool, bool, Option<T>)> pub, ISubscribe<(bool, bool, Option<T>)> sub)) (subject, subject);
            var res = sub;

            s1.Subscribe(() => pub.Publish((true, default, default)), scope);
            s2.Subscribe(() => pub.Publish((default, true, default)), scope);
            s3.Subscribe(msg => pub.Publish((default, default, msg)), scope);

            return res;
        }

        public static ISubscribe<(bool, bool, bool)> BranchOf(this IScope scope, ISubscribe s1, ISubscribe s2, ISubscribe s3)
        {
            var (pub, sub) = scope.PubSub<(bool, bool, bool)>();

            s1.Subscribe(() => pub.Publish((true, default, default)), scope);
            s2.Subscribe(() => pub.Publish((default, true, default)), scope);
            s3.Subscribe(() => pub.Publish((default, default, true)), scope);

            return sub;
        }

        public static ISubscribe<(bool, bool)> BranchOf(this IScope scope, ISubscribe s1, ISubscribe s2)
        {
            var (pub, sub) = scope.PubSub<(bool, bool)>();

            s1.Subscribe(() => pub.Publish((true, default)), scope);
            s2.Subscribe(() => pub.Publish((default, true)), scope);

            return sub;
        }

        [MustUseReturnValue]
        public static IDisposable SubScope(this IScope outer, out IScope res)
        {
            var dispose = React.Scope(out res);
            Action disposal = dispose.Dispose;
            res.Subscribe(_Remove);
            outer.Subscribe(disposal);
            return dispose;

            void _Remove()
            {
                if (!outer.Disposing)
                    outer.Unsubscribe(disposal);
            }
        }

        internal static void Subscribe(this IScope target, Action dispose, IScope unsubScope)
        {
            target.Subscribe(dispose);
            unsubScope.Subscribe(_Remove);

            void _Remove() => target.Unsubscribe(dispose);
        }
    }
}