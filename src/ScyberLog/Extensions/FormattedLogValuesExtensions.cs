using System;
using Scyberlog.Utils;

namespace ScyberLog
{
    internal static class FormattedLogValuesExtensions
    {
        public static string GetOriginalMessage(this object formattedLogValues)
        {
            if (!IsFormattedLogValues(formattedLogValues)) { return string.Empty; }
            return formattedLogValues.GetPrivateField<string>("_originalMessage") ?? string.Empty;
        }

        public static object[] GetValues(this object formattedLogValues)
        {
            if (!IsFormattedLogValues(formattedLogValues)) { return Array.Empty<object>(); }
            return formattedLogValues.GetPrivateField<object[]>("_values") ?? Array.Empty<object>();
        }

        public static bool IsFormattedLogValues(this object @object)
        {
            return @object.GetType().FullName == "Microsoft.Extensions.Logging.FormattedLogValues";
        }
    }
}