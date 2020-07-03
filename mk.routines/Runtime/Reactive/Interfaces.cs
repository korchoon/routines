// ----------------------------------------------------------------------------
// Async Reactors framework https://github.com/korchoon/async-reactors
// Copyright (c) 2016-2019 Mikhail Korchun <korchoon@gmail.com>
// ----------------------------------------------------------------------------

using System;

namespace Mk.Routines
{
    public interface IScope
    {
        bool Disposing { get; }
        bool Disposed { get; }
        void Subscribe(Action dispose);
        void Unsubscribe(Action dispose);
    }

    public interface IPublish
    {
        void Publish();
    }

    public interface ISubscribe
    {
        void Subscribe(Action callback, IScope scope);
    }

    public interface IPublish<in T>
    {
        void Publish(T msg);
    }

    public interface ISubscribe<out T>
    {
        void Subscribe(Action<T> callback, IScope scope);
    }
}