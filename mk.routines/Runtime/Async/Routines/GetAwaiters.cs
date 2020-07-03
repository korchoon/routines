// ----------------------------------------------------------------------------
// Async Reactors framework https://github.com/korchoon/async-reactors
// Copyright (c) 2016-2019 Mikhail Korchun <korchoon@gmail.com>
// ----------------------------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Mk.Routines
{
    public static class GetAwaiters
    {
        static ISubscribe DefaultSch => Sch.Update;
        static IScope DefaultScope => Sch.Scope;

        public static Routine Convert(Func<CancellationToken, Task> factory, IScope scope)
        {
            var cts = new CancellationTokenSource();
            var tt = factory.Invoke(cts.Token);
            var routine = Inner(tt);
            routine.GetScope(scope).Subscribe(cts.Dispose);
            return routine;

            // todo
            async Routine Inner(Task t)
            {
                var aw = t.ConfigureAwait(true).GetAwaiter();
                while (!aw.IsCompleted)
                    await Sch.Update;

                aw.GetResult();
            }
        }

        public static Routine<T> Convert<T>(Func<CancellationToken, Task<T>> factory, IScope scope)
        {
            var cts = new CancellationTokenSource();
            var tt = factory.Invoke(cts.Token);
            var routine = Inner(tt);
            scope.Subscribe(Dispose);
            routine.Scope.Subscribe(cts.Dispose);
            return routine;

            void Dispose()
            {
                routine.ToOptional().Dispose();
            }

            // todo
            async Routine<T> Inner(Task<T> t)
            {
                var aw = t.ConfigureAwait(true).GetAwaiter();
                while (!aw.IsCompleted)
                    await Sch.Update;

                return aw.GetResult();
            }
        }

        public static Awaiter<T> GetAwaiter<T>(this ISubscribe<T> s)
        {
            var result = new Option<T>();
            var d1 = React.Scope(out var scope);
            var aw = new Awaiter(scope, d1.Dispose);
            var res = new Awaiter<T>(aw, () => result.GetOrFail());
            s.Subscribe(msg =>
            {
                result = msg;
                d1.Dispose();
            }, scope);
            return res;
        }

        public static Awaiter GetAwaiter(this ISubscribe s)
        {
            var d1 = React.Scope(out var scope);
            var aw = new Awaiter(scope, d1.Dispose);
            s.Subscribe(d1.Dispose, scope);
            return aw;
        }

        public static Awaiter GetAwaiter(this IScope outer)
        {
            return new Awaiter(outer, Empty.Action());
        }

        public static Awaiter GetAwaiter(this float sec) => Delay(sec, DefaultSch).GetAwaiter();

        public static Awaiter GetAwaiter(this int sec) => GetAwaiter((float) sec);
        public static Awaiter GetAwaiter(this double sec) => GetAwaiter((float) sec);

        class Never : IScope
        {
            internal static Never Instance { get; } = new Never();

            public bool Disposing { get; }
            public bool Disposed { get; }

            public void Subscribe(Action dispose)
            {
            }

            public void Unsubscribe(Action dispose)
            {
            }
        }
        static bool KeepWaiting(this float endTime, float currentTime) => endTime > currentTime;

        static IScope Delay(float seconds, ISubscribe s)
        {
            if (DefaultScope.Disposing)
                return Never.Instance;

            var pub = DefaultScope.SubScope(out var res);
            var timer = Time.time + seconds;

            s.Subscribe(() =>
            {
                if (timer.KeepWaiting(Time.time)) return;
                
                pub.Dispose();
            }, res);

            return res;
        }
    }
}