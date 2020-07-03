// ----------------------------------------------------------------------------
// Async Reactors framework https://github.com/korchoon/async-reactors
// Copyright (c) 2016-2019 Mikhail Korchun <korchoon@gmail.com>
// ----------------------------------------------------------------------------

using System;

namespace Mk.Routines
{
    public static class RoutineApi
    {
        public static ISubscribe<T> ToSub<T>(Func<IPublish<T>, Routine> ctor, IScope scope)
        {
            IPublish<T> publish;
            ISubscribe<T> sub;
            (publish, sub) = scope.PubSub<T>();
            var res = sub;
            ctor.Invoke(publish).DisposeOn(scope);
            return res;
        }

        public static ISubscribe ToSub(this Routine r, IScope scope)
        {
            var (pub, sub) = scope.PubSub();

            // will implicitly dispose r
            Local().DisposeOn(scope);
            using (IDisposable dispose = React.Scope(out IScope onDispose))
            {
                
            }

            return sub;


            async Routine Local()
            {
                try
                {
                    await sub;
                    await r;
                }
                finally
                {
                    pub.Publish();
                }
            }
        }

        public static void Every(float timeout, Action action, IScope scope)
        {
            Inner().DisposeOn(scope);

            async Routine Inner()
            {
                while (true)
                {
                    await timeout;
                    action();
                }
            }
        }

        [MustUseReturnValue]
        public static Routine<T>.Optional DisposeOn<T>(this Routine<T> r, IScope scope)
        {
            return r.ToOptional().DisposeOn(scope);
        }

        public static T1 DisposeOn<T1>(this T1 dispose, IScope scope) where T1 : IDisposable
        {
            scope.Subscribe(dispose.Dispose);
            return dispose;
        }
    }
}