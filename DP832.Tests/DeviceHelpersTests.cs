using System.Collections.Generic;
using DP832PowerSupply;
using Xunit;

namespace DP832.Tests
{
    public class DeviceHelpersTests
    {
        // ── ParseProtectionState ─────────────────────────────────────────────────

        [Theory]
        [InlineData("ON",    true)]
        [InlineData("on",    true)]
        [InlineData("On",    true)]
        [InlineData("1",     true)]
        [InlineData(" ON ",  true)]   // leading/trailing whitespace
        [InlineData(" 1 ",   true)]
        public void ParseProtectionState_EnabledValues_ReturnsTrue(string input, bool expected)
        {
            Assert.Equal(expected, DeviceHelpers.ParseProtectionState(input));
        }

        [Theory]
        [InlineData("OFF",   false)]
        [InlineData("off",   false)]
        [InlineData("0",     false)]
        [InlineData(" OFF ", false)]  // leading/trailing whitespace
        [InlineData("",      false)]
        [InlineData("2",     false)]
        [InlineData("TRUE",  false)]  // only "ON" and "1" are truthy
        public void ParseProtectionState_DisabledValues_ReturnsFalse(string input, bool expected)
        {
            Assert.Equal(expected, DeviceHelpers.ParseProtectionState(input));
        }

        // ── GetChannelMaxVoltage ─────────────────────────────────────────────────

        [Theory]
        [InlineData(1, 30.0)]
        [InlineData(2, 30.0)]
        [InlineData(3,  5.0)]
        public void GetChannelMaxVoltage_ReturnsCorrectLimit(int channel, double expected)
        {
            Assert.Equal(expected, DeviceHelpers.GetChannelMaxVoltage(channel));
        }

        // ── GetChannelMaxCurrent ─────────────────────────────────────────────────

        [Fact]
        public void GetChannelMaxCurrent_Returns3Amps()
        {
            Assert.Equal(3.0, DeviceHelpers.GetChannelMaxCurrent());
        }

        // ── IsValidVoltage ───────────────────────────────────────────────────────

        [Theory]
        [InlineData(0.0,  1, true)]
        [InlineData(15.0, 1, true)]
        [InlineData(30.0, 1, true)]
        [InlineData(0.0,  3, true)]
        [InlineData(5.0,  3, true)]
        public void IsValidVoltage_ValidValues_ReturnsTrue(double voltage, int channel, bool expected)
        {
            Assert.Equal(expected, DeviceHelpers.IsValidVoltage(voltage, channel));
        }

        [Theory]
        [InlineData(-0.1, 1, false)]
        [InlineData(30.1, 1, false)]
        [InlineData(5.1,  3, false)]
        [InlineData(-1.0, 3, false)]
        public void IsValidVoltage_OutOfRange_ReturnsFalse(double voltage, int channel, bool expected)
        {
            Assert.Equal(expected, DeviceHelpers.IsValidVoltage(voltage, channel));
        }

        // ── IsValidCurrent ───────────────────────────────────────────────────────

        [Theory]
        [InlineData(0.0,  true)]
        [InlineData(1.5,  true)]
        [InlineData(3.0,  true)]
        public void IsValidCurrent_ValidValues_ReturnsTrue(double current, bool expected)
        {
            Assert.Equal(expected, DeviceHelpers.IsValidCurrent(current));
        }

        [Theory]
        [InlineData(-0.1, false)]
        [InlineData(3.1,  false)]
        public void IsValidCurrent_OutOfRange_ReturnsFalse(double current, bool expected)
        {
            Assert.Equal(expected, DeviceHelpers.IsValidCurrent(current));
        }

        // ── IsValidOvpLevel ──────────────────────────────────────────────────────

        [Theory]
        [InlineData(0.01, 1, true)]
        [InlineData(15.0, 1, true)]
        [InlineData(31.0, 1, true)]   // max+1 is the upper bound
        [InlineData(0.01, 3, true)]
        [InlineData(6.0,  3, true)]   // CH3 max+1
        public void IsValidOvpLevel_ValidValues_ReturnsTrue(double level, int channel, bool expected)
        {
            Assert.Equal(expected, DeviceHelpers.IsValidOvpLevel(level, channel));
        }

        [Theory]
        [InlineData(0.0,  1, false)]  // below minimum 0.01
        [InlineData(31.1, 1, false)]  // above CH1/CH2 max+1
        [InlineData(6.1,  3, false)]  // above CH3 max+1
        public void IsValidOvpLevel_OutOfRange_ReturnsFalse(double level, int channel, bool expected)
        {
            Assert.Equal(expected, DeviceHelpers.IsValidOvpLevel(level, channel));
        }

        // ── IsValidOcpLevel ──────────────────────────────────────────────────────

        [Theory]
        [InlineData(0.001, true)]
        [InlineData(1.5,   true)]
        [InlineData(4.0,   true)]   // max+1
        public void IsValidOcpLevel_ValidValues_ReturnsTrue(double level, bool expected)
        {
            Assert.Equal(expected, DeviceHelpers.IsValidOcpLevel(level));
        }

        [Theory]
        [InlineData(0.0,  false)]   // below minimum 0.001
        [InlineData(4.1,  false)]   // above max+1
        public void IsValidOcpLevel_OutOfRange_ReturnsFalse(double level, bool expected)
        {
            Assert.Equal(expected, DeviceHelpers.IsValidOcpLevel(level));
        }

        // ── FormatGpibAddress ────────────────────────────────────────────────────

        [Theory]
        [InlineData(1,  "GPIB0::1::INSTR")]
        [InlineData(5,  "GPIB0::5::INSTR")]
        [InlineData(30, "GPIB0::30::INSTR")]
        public void FormatGpibAddress_FormatsCorrectly(int deviceNumber, string expected)
        {
            Assert.Equal(expected, DeviceHelpers.FormatGpibAddress(deviceNumber));
        }

        // ── FormatTcpipAddress ───────────────────────────────────────────────────

        [Theory]
        [InlineData("192.168.1.100", "TCPIP::192.168.1.100::INSTR")]
        [InlineData("10.0.0.1",      "TCPIP::10.0.0.1::INSTR")]
        public void FormatTcpipAddress_FormatsCorrectly(string ip, string expected)
        {
            Assert.Equal(expected, DeviceHelpers.FormatTcpipAddress(ip));
        }

        // ── ParseStateFile ───────────────────────────────────────────────────────

        [Fact]
        public void ParseStateFile_ValidLines_ParsesKeyValuePairs()
        {
            var lines = new[]
            {
                "# DP832 State File",
                "# Saved: 2024-01-01 00:00:00",
                "",
                "CH1.Voltage=12.000",
                "CH1.Current=1.500",
                "CH1.OVPLevel=13.000",
                "CH1.OVPEnabled=True",
                "CH1.OCPLevel=2.000",
                "CH1.OCPEnabled=False",
                "CH1.OutputEnabled=False",
                "",
                "System.TrackMode=INDE",
                "System.OTP=True",
            };

            Dictionary<string, string> result = DeviceHelpers.ParseStateFile(lines);

            Assert.Equal("12.000", result["CH1.Voltage"]);
            Assert.Equal("1.500",  result["CH1.Current"]);
            Assert.Equal("13.000", result["CH1.OVPLevel"]);
            Assert.Equal("True",   result["CH1.OVPEnabled"]);
            Assert.Equal("2.000",  result["CH1.OCPLevel"]);
            Assert.Equal("False",  result["CH1.OCPEnabled"]);
            Assert.Equal("False",  result["CH1.OutputEnabled"]);
            Assert.Equal("INDE",   result["System.TrackMode"]);
            Assert.Equal("True",   result["System.OTP"]);
        }

        [Fact]
        public void ParseStateFile_CommentAndBlankLines_AreIgnored()
        {
            var lines = new[]
            {
                "# This is a comment",
                "",
                "   ",
                "Key=Value"
            };

            Dictionary<string, string> result = DeviceHelpers.ParseStateFile(lines);

            Assert.Single(result);
            Assert.Equal("Value", result["Key"]);
        }

        [Fact]
        public void ParseStateFile_KeyIsCaseInsensitive()
        {
            var lines = new[] { "ch1.voltage=5.000" };

            Dictionary<string, string> result = DeviceHelpers.ParseStateFile(lines);

            Assert.Equal("5.000", result["CH1.Voltage"]);
        }

        [Fact]
        public void ParseStateFile_ValueContainsEquals_PreservesValue()
        {
            // Edge case: value itself contains '='
            var lines = new[] { "Key=val=ue" };

            Dictionary<string, string> result = DeviceHelpers.ParseStateFile(lines);

            Assert.Equal("val=ue", result["Key"]);
        }

        [Fact]
        public void ParseStateFile_EmptyCollection_ReturnsEmptyDictionary()
        {
            Dictionary<string, string> result = DeviceHelpers.ParseStateFile(Array.Empty<string>());

            Assert.Empty(result);
        }
    }
}
