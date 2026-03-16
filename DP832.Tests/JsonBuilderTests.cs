using System.Collections.Generic;
using DP832.CLI;
using Xunit;

namespace DP832.Tests
{
    public class JsonBuilderTests
    {
        // ── Empty / single values ────────────────────────────────────────────────

        [Fact]
        public void Serialize_EmptyDictionary_ReturnsEmptyObject()
        {
            Assert.Equal("{}", JsonBuilder.Serialize(new Dictionary<string, object>()));
        }

        [Fact]
        public void Serialize_StringValue_ProducesQuotedValue()
        {
            var dict = new Dictionary<string, object> { ["name"] = "DP832" };
            Assert.Equal("{\"name\":\"DP832\"}", JsonBuilder.Serialize(dict));
        }

        [Fact]
        public void Serialize_BoolTrue_ProducesLowerCaseTrue()
        {
            var dict = new Dictionary<string, object> { ["active"] = true };
            Assert.Equal("{\"active\":true}", JsonBuilder.Serialize(dict));
        }

        [Fact]
        public void Serialize_BoolFalse_ProducesLowerCaseFalse()
        {
            var dict = new Dictionary<string, object> { ["active"] = false };
            Assert.Equal("{\"active\":false}", JsonBuilder.Serialize(dict));
        }

        [Fact]
        public void Serialize_IntValue_ProducesUnquotedInteger()
        {
            var dict = new Dictionary<string, object> { ["count"] = 42 };
            Assert.Equal("{\"count\":42}", JsonBuilder.Serialize(dict));
        }

        [Fact]
        public void Serialize_DoubleValue_ProducesThreeDecimalPlaces()
        {
            var dict = new Dictionary<string, object> { ["voltage"] = 12.0 };
            Assert.Equal("{\"voltage\":12.000}", JsonBuilder.Serialize(dict));
        }

        [Fact]
        public void Serialize_NullValue_ProducesJsonNull()
        {
            var dict = new Dictionary<string, object> { ["key"] = null };
            Assert.Equal("{\"key\":null}", JsonBuilder.Serialize(dict));
        }

        // ── Multiple keys ────────────────────────────────────────────────────────

        [Fact]
        public void Serialize_MultipleKeys_SeparatedByCommasNoSpaces()
        {
            var dict = new Dictionary<string, object> { ["a"] = 1, ["b"] = 2 };
            Assert.Equal("{\"a\":1,\"b\":2}", JsonBuilder.Serialize(dict));
        }

        // ── Nested types ─────────────────────────────────────────────────────────

        [Fact]
        public void Serialize_NestedDictionary_ProducesNestedObject()
        {
            var inner = new Dictionary<string, object> { ["voltage"] = 5.0 };
            var outer = new Dictionary<string, object> { ["ch3"] = inner };
            Assert.Equal("{\"ch3\":{\"voltage\":5.000}}", JsonBuilder.Serialize(outer));
        }

        [Fact]
        public void Serialize_ListOfDictionaries_ProducesJsonArray()
        {
            var list = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object> { ["ch"] = 1 },
                new Dictionary<string, object> { ["ch"] = 2 },
            };
            var dict = new Dictionary<string, object> { ["channels"] = list };
            Assert.Equal("{\"channels\":[{\"ch\":1},{\"ch\":2}]}", JsonBuilder.Serialize(dict));
        }

        [Fact]
        public void Serialize_EmptyList_ProducesEmptyArray()
        {
            var dict = new Dictionary<string, object> { ["items"] = new List<Dictionary<string, object>>() };
            Assert.Equal("{\"items\":[]}", JsonBuilder.Serialize(dict));
        }

        // ── String escaping ──────────────────────────────────────────────────────

        [Fact]
        public void Serialize_StringWithBackslash_EscapesBackslash()
        {
            var dict = new Dictionary<string, object> { ["path"] = @"C:\Temp" };
            Assert.Equal("{\"path\":\"C:\\\\Temp\"}", JsonBuilder.Serialize(dict));
        }

        [Fact]
        public void Serialize_StringWithDoubleQuote_EscapesQuote()
        {
            var dict = new Dictionary<string, object> { ["msg"] = "say \"hello\"" };
            Assert.Equal("{\"msg\":\"say \\\"hello\\\"\"}", JsonBuilder.Serialize(dict));
        }

        [Fact]
        public void Serialize_StringWithCarriageReturnAndNewline_EscapesBothControlChars()
        {
            var dict = new Dictionary<string, object> { ["msg"] = "line1\r\nline2" };
            Assert.Equal("{\"msg\":\"line1\\r\\nline2\"}", JsonBuilder.Serialize(dict));
        }

        // ── Unknown type fallback ─────────────────────────────────────────────────

        [Fact]
        public void Serialize_UnknownType_FallsBackToQuotedToString()
        {
            // long is not one of the explicitly handled types — falls back to SerializeString(value.ToString())
            var dict = new Dictionary<string, object> { ["val"] = 99L };
            Assert.Equal("{\"val\":\"99\"}", JsonBuilder.Serialize(dict));
        }
    }
}
