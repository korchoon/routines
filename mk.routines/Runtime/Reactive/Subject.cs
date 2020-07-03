// ----------------------------------------------------------------------------
// Async Reactors framework https://github.com/korchoon/async-reactors
// Copyright (c) 2016-2019 Mikhail Korchun <korchoon@gmail.com>
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Mk.Debugs;

namespace Mk.Routines
{
    class Subject : ISubscribe, IPublish
    {
        bool _completed;
        LinkedList<Action> _current;
        LinkedList<Action> _next;

        public Subject(IScope scope)
        {
            Asr.IsFalse(scope.Disposed);
            _current = new LinkedList<Action>();
            _next = new LinkedList<Action>();
            scope.Subscribe(Dispose);

            void Dispose()
            {
                if (CheckSetTrue(ref _completed)) return;

                _current.Clear();
                _next.Clear();
            }
        }

        public void Subscribe(Action callback, IScope scope)
        {
            if (_completed)
            {
                Asr.Fail("Tried to subscribe to ISub which is completed");
                return;
            }

            Asr.IsFalse(scope.Disposed);

            var node = _next.AddLast(callback);
            scope.Subscribe(Remove);

            void Remove()
            {
                node.List?.Remove(node);
            }
        }

        public void Publish()
        {
            if (_completed)
            {
                Asr.Fail("Tried to publish Next to IPub which is completed");
                return;
            }


            Asr.IsTrue(_current.Count == 0);
            Swap();
            Run(_current, _next);

            void Swap()
            {
                var tmp = _current;
                _current = _next;
                _next = tmp;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool CheckSetTrue(ref bool flag)
        {
            var copy = flag;
            flag = true;
            return copy;
        }

        static void Run(LinkedList<Action> current, LinkedList<Action> next)
        {
            Action a = null;
            while (TryDequeue(ref a)) a.Invoke();

            bool TryDequeue(ref Action value)
            {
                if (current.Count == 0)
                {
                    value = default;
                    return false;
                }

                var node = current.First;
                value = node.Value;
                current.RemoveFirst();
                next.AddLast(node);
                return true;
            }
        }
    }

    class Subject<T> : ISubscribe<T>, IPublish<T>
    {
        bool _completed;
        LinkedList<Action<T>> _current;
        LinkedList<Action<T>> _next;

        public Subject(IScope scope)
        {
            Asr.IsFalse(scope.Disposed);
            _current = new LinkedList<Action<T>>();
            _next = new LinkedList<Action<T>>();

            scope.Subscribe(Dispose);

            void Dispose()
            {
                if (Subject.CheckSetTrue(ref _completed))
                    return;

                _current.Clear();
                _next.Clear();
//                _current = null;
//                _next = null;
            }
        }

        public void Subscribe(Action<T> callback, IScope scope)
        {
            if (_completed)
            {
                Asr.Fail("Tried to subscribe to ISub which is completed");
                return;
            }

            Asr.IsFalse(scope.Disposed);
            var node = _next.AddLast(callback);
            scope.Subscribe(Remove);

            void Remove()
            {
                node.List?.Remove(node);
            }
        }

        public void Publish(T msg)
        {
            if (_completed)
            {
                Asr.Fail("Tried to publish Next to IPub which is completed");
                return;
            }

            Asr.IsTrue(_current.Count == 0);

            Swap();
            Run(_current, _next, msg);

            void Swap()
            {
                var tmp = _current;
                _current = _next;
                _next = tmp;
            }
        }

        static void Run(LinkedList<Action<T>> current, LinkedList<Action<T>> next, T msg)
        {
            Action<T> a = null;
            while (TryDequeue(ref a)) a.Invoke(msg);

            bool TryDequeue(ref Action<T> value)
            {
                if (current.Count == 0)
                {
                    value = default;
                    return false;
                }

                var node = current.First;
                value = node.Value;
                current.RemoveFirst();
                next.AddLast(node);
                return true;
            }
        }
    }
}