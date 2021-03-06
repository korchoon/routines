﻿// ----------------------------------------------------------------------------
// Async Reactors framework https://github.com/korchoon/async-reactors
// Copyright (c) 2016-2019 Mikhail Korchun <korchoon@gmail.com>
// ----------------------------------------------------------------------------

using System;
using System.Runtime.CompilerServices;
using System.Security;
using JetBrains.Annotations;
using Mk.Debugs;
using UnityEngine;

namespace Mk.Routines
{
    public class RoutineBuilder<T>
    {
        Action _continuation;
        IBreakable _inner;

        [UsedImplicitly] public Routine<T> Task { get; }

        RoutineBuilder(Routine<T> task)
        {
            Task = task;
            Task.Scope.Subscribe(BreakCurrent);
        }

        void BreakCurrent()
        {
            var i = _inner;
            if (i == null) return;
            
            _inner = null;
            i.Unsubscribe();
            i.BreakInner();
        }

        [UsedImplicitly]
        public static RoutineBuilder<T> Create() => new RoutineBuilder<T>(new Routine<T>());


        [UsedImplicitly]
        public void Start<TStateMachine>(ref TStateMachine stateMachine)
            where TStateMachine : IAsyncStateMachine
        {
            _continuation = stateMachine.MoveNext;
            _continuation.Invoke();
        }

        [UsedImplicitly]
        public void SetResult(T value)
        {
            Task.SetResult(value);
            Task.Dispose.Dispose();
        }

        [UsedImplicitly]
        public void SetException(Exception e)
        {
            Debug.LogException(e);
            SchPub.PublishError.Publish(e);
            Task.Dispose.Dispose();
        }

        [UsedImplicitly]
        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            switch (awaiter)
            {
                case IBreakable breakableAwaiter:
                    _inner = breakableAwaiter;
                    awaiter.OnCompleted(_continuation);
                    break;
                case SelfScopeAwaiter selfScopeAwaiter:
                    selfScopeAwaiter.Value = Task.Scope;
                    awaiter.OnCompleted(_continuation);
                    break;
                case SelfDisposeAwaiter selfDisposeAwaiter:
                    selfDisposeAwaiter.Value = Task.Dispose;
                    awaiter.OnCompleted(_continuation);
                    break;
                default:
                    Asr.Fail("passed unbreakable awaiter");
                    break;
            }
        }


        [SecuritySafeCritical, UsedImplicitly]
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion where TStateMachine : IAsyncStateMachine =>
            AwaitOnCompleted(ref awaiter, ref stateMachine);


        [UsedImplicitly]
        public void SetStateMachine(IAsyncStateMachine stateMachine) => _continuation = stateMachine.MoveNext;
    }
}