// ----------------------------------------------------------------------------
// Async Reactors framework https://github.com/korchoon/async-reactors
// Copyright (c) 2016-2019 Mikhail Korchun <korchoon@gmail.com>
// ----------------------------------------------------------------------------

namespace Mk.Routines
{
    interface IBreakable
    {
        void BreakInner();
        void Unsubscribe();
    }
}