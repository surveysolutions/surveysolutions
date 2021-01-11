namespace StatData.Core
{
    /// <summary>
    /// Type of script most appropriate for the text
    /// </summary>
    public enum DatasetScript
    {
        Western,
        Hebrew,
        Arabic,
        Greek,
        Turkish,
        Baltic,
        CentralEuropean,
        Cyrillic,
        Vietnamese,
        Other
    };

    class DatasetEncoding
    {
        public static int CodePage(DatasetScript script)
        {
            switch (script)
            {
                case DatasetScript.Western:
                    return 1252;
                case DatasetScript.Hebrew:
                    return 1255;
                case DatasetScript.Arabic:
                    return 1256;
                case DatasetScript.Greek:
                    return 1253;
                case DatasetScript.Turkish:
                    return 1254;
                case DatasetScript.Baltic:
                    return 1257;
                case DatasetScript.CentralEuropean:
                    return 1250;
                case DatasetScript.Cyrillic:
                    return 1251;
                case DatasetScript.Vietnamese:
                    return 1258;
                default:
                    return CodePage(DatasetScript.Western);
            }
        }

        public static string CodepageName(int codepage)
        {
            switch (codepage)
            {
                case 1252:
                    return "ANSI-1252: Western (Latin 1)";
                case 1255:
                    return "ANSI-1255: Hebrew";
                case 1256:
                    return "ANSI-1256: Arabic";
                case 1253:
                    return "ANSI-1253: Greek";
                case 1254:
                    return "ANSI-1254: Turkish";
                case 1257:
                    return "ANSI-1257: Baltic";
                case 1250:
                    return "ANSI-1250: Central European (Latin 2)";
                case 1251:
                    return "ANSI-1251: Cyrillic";
                case 1258:
                    return "ANSI-1258: Vietnamese";
                case 65001:
                    return "UTF-8 Unicode";
                default:
                    return "Other/Unknown";
            }
        }
    }
}
