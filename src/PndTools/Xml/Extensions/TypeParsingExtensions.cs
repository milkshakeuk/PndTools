using System;

namespace PndTools.Xml.Extensions
{
    public static class TypeParsingExtensions
    {
        public static T Parse<T>(string value)
        {
            var typ = typeof(T);
            if (typ == typeof(string))
            {
                return (T)(object)value;
            }

            // Reference types are allowed to be null, whereas value types will cause
            // this method to throw if not present in the field
            if (string.IsNullOrEmpty(value))
            {
                if (typ.IsClass || (Nullable.GetUnderlyingType(typ) != null))
                {
                    return (T)(object)null;
                }

                throw new NullReferenceException($"The field didn't contain a value, and {typ.Name} cannot be cast to null");
            }

            if ((typ == typeof(bool)) || (typ == typeof(bool?)))
            {
                return (T)(object)bool.Parse(value);
            }

            if ((typ == typeof(int)) || (typ == typeof(int?)))
            {
                return (T)(object)int.Parse(value);
            }

            if ((typ == typeof(double)) || (typ == typeof(double?)))
            {
                return (T)(object)double.Parse(value);
            }

            if ((typ == typeof(decimal)) || (typ == typeof(decimal?)))
            {
                return (T)(object)decimal.Parse(value);
            }

            if ((typ == typeof(DateTime)) || (typ == typeof(DateTime?)))
            {
                return (T)(object)DateTime.Parse(value);
            }

            if ((typ == typeof(Guid)) || (typ == typeof(Guid?)))
            {
                return (T)(object)Guid.Parse(value);
            }

            throw new InvalidOperationException($"Unable to parse type {typ}");
        }
    }
}