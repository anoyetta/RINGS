using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace aframe
{
    public static class EnumExtensions
    {
        public static string ToDisplayName<T>(
            this T e) where T : Enum
        {
            var name = e.ToString();

            var fi = e.GetType().GetField(e.ToString());
            var attrs = fi.GetCustomAttributes(
                typeof(DisplayAttribute),
                false) as DisplayAttribute[];

            if (attrs != null &&
                attrs.Length > 0)
            {
                name = attrs[0].Name;
            }

            return name;
        }

        public static string ToName<T>(
            this T e) where T : Enum
            => Enum.GetName(typeof(T), e);

        public static int ToInt<T>(
            this T e) where T : Enum
            => Convert.ToInt32(e);
    }

    public static class EnumConverter
    {
        public static T FromString<T>(
            string text) where T : Enum
            => (T)Enum.Parse(typeof(T), text, true);

        public static T FromInt<T>(
            int value) where T : Enum
            => (T)Enum.ToObject(typeof(T), value);

        public static IEnumerable<T> ToEnumerable<T>(
            params T[] ignores) where T : Enum
        {
            var values = default(IEnumerable<T>);
            values = (IEnumerable<T>)Enum.GetValues(typeof(T));

            if (ignores != null &&
                ignores.Length > 0)
            {
                values = values.Where(x => !ignores.Contains(x));
            }

            return values;
        }

        public static IEnumerable<EnumContainer<T>> ToEnumerableContainer<T>(
            params T[] ignores) where T : Enum
        {
            var values = default(IEnumerable<T>);
            values = (IEnumerable<T>)Enum.GetValues(typeof(T));

            if (ignores != null &&
                ignores.Length > 0)
            {
                values = values.Where(x => !ignores.Contains(x));
            }

            return values.Select(x => new EnumContainer<T>(x));
        }
    }

    public class EnumContainer<T> where T : Enum
    {
        public EnumContainer(
            T value)
        {
            this.Value = value;
        }

        public T Value { get; set; }

        public string Display => this.Value.ToDisplayName();

        public override string ToString()
            => this.Display;
    }
}
