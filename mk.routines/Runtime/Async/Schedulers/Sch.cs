// ----------------------------------------------------------------------------
// Async Reactors framework https://github.com/korchoon/async-reactors
// Copyright (c) 2016-2019 Mikhail Korchun <korchoon@gmail.com>
// ----------------------------------------------------------------------------

using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Mk.Tests")]

namespace Mk.Routines
{
    public static class SchPub
    {
        internal static IPublish<Exception> PublishError;
    }
    
    public static class Sch
    {
        public static ISubscribe<Exception> OnError { get; internal set; }
        public static ISubscribe Update { get; internal set; }
        public static IScope Scope { get; internal set; }

        public static ISubscribe LateUpdate { get; internal set; }
        public static ISubscribe<float> UpdateTime { get; set; }

        public static ISubscribe Physics { get; internal set; }
        public static ISubscribe<float> PhysicsTime { get; internal set; }

        public static ISubscribe Logic { get; internal set; }
    }
}