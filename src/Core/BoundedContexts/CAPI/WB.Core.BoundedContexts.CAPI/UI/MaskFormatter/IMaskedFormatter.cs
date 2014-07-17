using System;

namespace WB.Core.BoundedContexts.Capi.UI.MaskFormatter
{
    public interface IMaskedFormatter {
        string Mask { get; }
        String ValueToString(string value, ref int oldCurstorPosition);
        bool IsTextMaskMatched(string text);
        string GetCleanText(string text);
    }
}