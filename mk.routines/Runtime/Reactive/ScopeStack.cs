// ----------------------------------------------------------------------------
// Async Reactors framework https://github.com/korchoon/async-reactors
// Copyright (c) 2016-2019 Mikhail Korchun <korchoon@gmail.com>
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Mk.Debugs;

namespace Mk.Routines
{
    class ScopeStack : IDisposable, IScope
    {
        LinkedList<Action> _stack;
        public bool Disposing { get; private set; }

        public ScopeStack()
        {
            Disposed = false;
            Disposing = false;
            _stack = new LinkedList<Action>();
        }

        public void Dispose()
        {
            if (Disposed) return;

            if (Disposing)
            {
//                Asr.Fail("Self-looping dispose call");
                return;
            }

            Disposing = true;
            Action cur = null;
            while (TryDequeue(ref cur))
            {
                Asr.IsFalse(Disposed);
                cur.Invoke();
            }

            _stack.Clear();
            _stack = null;
            Disposed = true;

        }

        bool TryDequeue(ref Action value)
        {
            if (_stack.Count == 0) return false;

            value = _stack.Last.Value;
            _stack.RemoveLast();
            return true;
        }

        public bool Disposed { get; private set; }

        public void Subscribe(Action dispose)
        {
            if (Disposed || Disposing)
            {
                dispose.Invoke();
                return;
            }

            _stack.AddLast(dispose);
        }

        public void Unsubscribe(Action dispose)
        {
            if (Disposed || Disposing)
            {
                Asr.Fail("Cannot unsubscribe during or after disposal");
                return;
            }

            var any = _stack.Remove(dispose);
            Asr.IsTrue(any, "Delegate not found: make sure it's the same which was passed to OnDispose");
        }
    }
}