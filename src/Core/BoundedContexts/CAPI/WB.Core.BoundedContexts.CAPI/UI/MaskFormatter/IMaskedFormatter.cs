using System;

namespace WB.Core.BoundedContexts.Capi.UI.MaskFormatter
{
    public interface IMaskedFormatter {
        string Mask { get; }
        string FormatValue(string value, ref int oldCursorPosition);
        bool IsTextMaskMatched(string text);
    }
}