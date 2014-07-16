using System;

namespace WB.UI.Shared.Android.Controls.MaskedEditTextControl
{
    public interface IMaskedFormatter {
        string Mask { get; }
        String ValueToString(string value, ref int oldCurstorPosition);
        bool IsTextMaskMatched(string text);
        string GetCleanText(string text);
    }
}