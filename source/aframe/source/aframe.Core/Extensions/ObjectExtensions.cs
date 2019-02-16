using System.Linq;

namespace aframe
{
    public static class ObjectExtensions
    {
        public static T Clone<T>(
            this T source)
            where T : class
            => typeof(T).InvokeMember(
              "MemberwiseClone",
              System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.InvokeMethod,
              null, source, null) as T;

        public static dynamic Clone(
            this object source)
            => source.GetType().InvokeMember(
              "MemberwiseClone",
              System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.InvokeMethod,
              null, source, null);

        public static void CopyProperties(
            this object source,
            object destination,
            params string[] ignoreProperties)
        {
            var pis = source.GetType().GetProperties();
            foreach (var pi in pis)
            {
                if (ignoreProperties != null)
                {
                    if (ignoreProperties.Contains(pi.Name))
                    {
                        continue;
                    }
                }

                var value = pi.GetValue(
                    source);

                if (value != null)
                {
                    pi.SetValue(destination, value);
                }
            }
        }
    }
}
