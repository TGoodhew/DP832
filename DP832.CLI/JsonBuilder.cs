using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace DP832.CLI
{
    /// <summary>
    /// Minimal JSON serialiser for flat and shallow-nested output objects produced by CLI commands.
    /// Supports <see cref="bool"/>, <see cref="int"/>, <see cref="double"/>, <see cref="string"/>,
    /// nested <c>Dictionary&lt;string,object&gt;</c>, and <c>List&lt;Dictionary&lt;string,object&gt;&gt;</c> values.
    /// </summary>
    internal static class JsonBuilder
    {
        /// <summary>Serialises an ordered dictionary to a compact JSON object string.</summary>
        public static string Serialize(Dictionary<string, object> obj)
        {
            var sb = new StringBuilder();
            sb.Append("{");
            bool first = true;
            foreach (var kv in obj)
            {
                if (!first) sb.Append(",");
                first = false;
                sb.Append(SerializeString(kv.Key));
                sb.Append(":");
                sb.Append(SerializeValue(kv.Value));
            }
            sb.Append("}");
            return sb.ToString();
        }

        private static string SerializeValue(object value)
        {
            if (value == null) return "null";
            if (value is bool) return ((bool)value) ? "true" : "false";
            if (value is int) return ((int)value).ToString(CultureInfo.InvariantCulture);
            if (value is double) return ((double)value).ToString("F3", CultureInfo.InvariantCulture);
            if (value is string) return SerializeString((string)value);
            var asList = value as List<Dictionary<string, object>>;
            if (asList != null)
            {
                var sb = new StringBuilder();
                sb.Append("[");
                for (int i = 0; i < asList.Count; i++)
                {
                    if (i > 0) sb.Append(",");
                    sb.Append(Serialize(asList[i]));
                }
                sb.Append("]");
                return sb.ToString();
            }
            var asDict = value as Dictionary<string, object>;
            if (asDict != null) return Serialize(asDict);
            return SerializeString(value.ToString());
        }

        private static string SerializeString(string s)
        {
            return "\"" + s
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\r", "\\r")
                .Replace("\n", "\\n") + "\"";
        }
    }
}
