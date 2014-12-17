namespace WB.Core.BoundedContexts.Capi.UI.MaskFormatter
{
    public class EmptyMaskFormatter : IMaskedFormatter
    {
        public string Mask
        {
            get { return string.Empty; }
        }

        public string FormatValue(string value, ref int oldCursorPosition)
        {
            return value ?? "";
        }

        public bool IsTextMaskMatched(string text)
        {
            return true;
        }
    }
}