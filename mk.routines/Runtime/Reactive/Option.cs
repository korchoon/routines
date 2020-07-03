// ----------------------------------------------------------------------------
// Async Reactors framework https://github.com/korchoon/async-reactors
// Copyright (c) 2016-2019 Mikhail Korchun <korchoon@gmail.com>
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Mk.Debugs;
using UnityEngine;

namespace Mk.Routines
{
    [Serializable,]
    public struct Option<T> : IEquatable<Option<T>>, IComparable<Option<T>>
    {
        // ReSharper disable once StaticMemberInGenericType
        internal static readonly bool IsValueType;

        [SerializeField] public bool HasValue { get; private set; }

        [SerializeField] T Value { get; set; }

        public static implicit operator Option<T>(T arg)
        {
            if (!IsValueType) return ReferenceEquals(arg, null) ? new Option<T>() : Option.Some(arg);

#if M_WARN
            if (arg.Equals(default(T)))
            {
                Warn.Warning($"{arg} has default value");
            }
#endif

            return Option.Some(arg);
        }

        static Option()
        {
            IsValueType = typeof(T).IsValueType;
        }

        public void GetOrFail(out T value)
        {
            if (!TryGet(out value))
                Asr.Fail($"Option<{typeof(T).Name}> has no value");
        }

        public T GetOrFail()
        {
            if (!TryGet(out var value))
                Asr.Fail($"Option<{typeof(T).Name}> has no value");
            return value;
        }

        public bool TryGet(out T value)
        {
            if (!HasValue)
            {
                value = default(T);
                return false;
            }

            value = Value;
            return true;
        }

        internal Option(T value, bool hasValue)
        {
            Value = value;
            HasValue = hasValue;
        }

        public T ValueOr(T alternative)
        {
            return HasValue ? Value : alternative;
        }

        public override string ToString()
        {
            if (!HasValue) return "None";

            return Value == null ? "Some(null)" : $"Some({Value})";
        }

        #region eq comparers boilerplate

        public bool Equals(Option<T> other)
        {
            if (!HasValue && !other.HasValue)
                return true;

            if (HasValue && other.HasValue)
                return EqualityComparer<T>.Default.Equals(Value, other.Value);

            return false;
        }

        public override bool Equals(object obj)
        {
            return obj is Option<T> && Equals((Option<T>) obj);
        }

        public static bool operator ==(Option<T> left, Option<T> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Option<T> left, Option<T> right)
        {
            return !left.Equals(right);
        }

        public override int GetHashCode()
        {
            if (!HasValue) return 0;

            return Option.IsNotNull(Value) ? Value.GetHashCode() : 1;
        }

        public int CompareTo(Option<T> other)
        {
            if (HasValue && !other.HasValue) return 1;
            if (!HasValue && other.HasValue) return -1;

            return Comparer<T>.Default.Compare(Value, other.Value);
        }

        public static bool operator <(Option<T> left, Option<T> right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator <=(Option<T> left, Option<T> right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >(Option<T> left, Option<T> right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator >=(Option<T> left, Option<T> right)
        {
            return left.CompareTo(right) >= 0;
        }

        #endregion
    }

    public static class Option
    {
        public static Option<T> Unwrap<T>(this Option<Option<T>> opt) => opt.TryGet(out var res1) ? res1 : default;

        public static bool UnwrapTryGet<T>(this Option<Option<T>> opt, out T value)
        {
            if (!opt.TryGet(out var res1))
            {
                value = default;
                return false;
            }

            if (!res1.TryGet(out var res2))
            {
                value = default;
                return false;
            }

            value = res2;
            return true;
        }

        public static Option<T> Some<T>(T value) => new Option<T>(value, true);
        public static Option<T> None<T>() => new Option<T>(default, false);

        public static bool IsNotNull<T>(T t)
        {
            return Option<T>.IsValueType || t != null;
        }
    }
}