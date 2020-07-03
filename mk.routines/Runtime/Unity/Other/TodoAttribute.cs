// ----------------------------------------------------------------------------
// Async Reactors framework https://github.com/korchoon/async-reactors
// Copyright (c) 2016-2019 Mikhail Korchun <korchoon@gmail.com>
// ----------------------------------------------------------------------------

using System;

namespace Mk.Routines
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.All | AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false, Inherited = true )]
    public class TodoAttribute : Attribute
    {
    }
}