﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace WB.Core.BoundedContexts.Headquarters
{
    public static class Utils
    {
        private static readonly Regex RemoveNewLineRegEx = new Regex(@"\t|\n|\r", RegexOptions.Compiled);
        public static string RemoveNewLine(string value)
        {
            return string.IsNullOrEmpty(value) ? string.Empty : RemoveNewLineRegEx.Replace(value, " ");
        }
        
        public static long InKb(this long bytes)
        {
            return bytes / 1024;
        }
    }
}
