// ----------------------------------------------------------------------------
// Async Reactors framework https://github.com/korchoon/async-reactors
// Copyright (c) 2016-2019 Mikhail Korchun <korchoon@gmail.com>
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using Mk.Debugs;

namespace Mk.Routines
{
    public class MethodEntry<T> : MethodEntry
    {
        public T Value;

        public void Trace(T value)
        {
            Inc();
            Value = value;
        }
    }

    public class MethodEntry
    {
        public int Count { get; private set; }
        public List<StackTraceHolder> List;

        public MethodEntry()
        {
            List = new List<StackTraceHolder>();
        }

        public void Inc(int skipFrames = 1)
        {
            List.Add(StackTraceHolder.New(skipFrames + 1));
            ++Count;
        }
    }
}