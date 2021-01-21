using System;

namespace StatData.Writers.Spss
{
    internal class SpssVarFormat
    {
        private readonly Byte _decimals;
        private readonly Byte _width;
        private readonly Byte _format;

        internal SpssVarFormat(byte decimals, byte width, byte format)
        {
            _decimals = decimals;
            _width = width;
            _format = format;
        }

        internal Int32 Value
        {
            get { return _decimals + 256*_width + 256*256*_format; }
        }

        internal static SpssVarFormat DefaultNumericFormat
        {
            get { return new SpssVarFormat(3, 12, 5); }
        }

        internal static SpssVarFormat DefaultDateFormat
        {
            get { return new SpssVarFormat(0, 10, 39); }
        }

        internal static SpssVarFormat DefaultDateTimeFormat
        {
            get { return new SpssVarFormat(0, 20, 22); }
        }

        internal static SpssVarFormat NumericDecimalsFormat(int decimals)
        {
            if (decimals < 0 || decimals > 15)
                throw new ArgumentOutOfRangeException(
                    "decimals", decimals, "Parameter decimals is out of range");
            return new SpssVarFormat((byte) decimals, 12, 5);
        }
    }
}
