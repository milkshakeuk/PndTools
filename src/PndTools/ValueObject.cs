using System;
using System.Collections.Generic;
using System.Reflection;

namespace PndTools
{
    /// <summary>
    ///     Generic value object equality class
    ///     Based on https://lostechies.com/jimmybogard/2007/06/25/generic-value-object-equality/
    ///     Incldues fix for equality comparisons on subclasses
    ///     source: https://gist.github.com/rowandh/4760416254ebc48e0780#file-valueobject-cs
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal abstract class ValueObject<T> : IEquatable<T> where T : ValueObject<T>
    {
        public static bool operator ==(ValueObject<T> x, ValueObject<T> y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (((object)x == null) || ((object)y == null))
            {
                return false;
            }

            return x.Equals(y);
        }

        public static bool operator !=(ValueObject<T> x, ValueObject<T> y)
        {
            return !(x == y);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            var other = obj as T;

            return Equals(other);
        }

        public virtual bool Equals(T other)
        {
            if (other == null)
            {
                return false;
            }

            var type = GetType();

            var otherType = other.GetType();

            if (type != otherType)
            {
                return false;
            }

            var fields = GetFields();

            foreach (var field in fields)
            {
                var value1 = field.GetValue(other);
                var value2 = field.GetValue(this);

                if (value1 == null)
                {
                    if (value2 != null)
                    {
                        return false;
                    }
                }
                else if (!value1.Equals(value2))
                {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            var fields = GetFields();

            const int startValue = 17;
            const int multiplier = 59;

            var hashCode = startValue;

            foreach (var field in fields)
            {
                var value = field.GetValue(this);

                if (value != null)
                {
                    hashCode = (hashCode * multiplier) + value.GetHashCode();
                }
            }

            return hashCode;
        }

        private IEnumerable<FieldInfo> GetFields()
        {
            var type = GetType();

            var fields = new List<FieldInfo>();

            while (type != typeof(object))
            {
                if (type == null)
                {
                    continue;
                }

                fields.AddRange(type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public));

                type = type.GetTypeInfo().BaseType;
            }

            return fields;
        }
    }
}