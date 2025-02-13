using System;
using static System.Reflection.BindingFlags;

namespace Scyberlog.Utils
{
    internal static class ObjectExtensions
    {
        internal static T GetPrivateField<T>(this object @object, string fieldName)
        {
            var field = @object.GetType().GetField(fieldName, NonPublic | Instance);
            if (field == null) { throw new ArgumentException($"Field {fieldName} not found on type {@object.GetType().FullName}"); }
            return (T)field.GetValue(@object);
        }

        internal static T With<T>(this T @object, Action<T> action)
        {
            action?.Invoke(@object);
            return @object;
        }
    }
}