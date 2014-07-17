namespace WB.Core.BoundedContexts.Capi.UI.MaskFormatter
{
    public class EmptyMaskFormatter : IMaskedFormatter
    {
        public string Mask
        {
            get { return string.Empty; }
        }

        public string ValueToString(string value, ref int oldCurstorPosition)
        {
            return value ?? "";
        }

        public bool IsTextMaskMatched(string text)
        {
            return true;
        }

        public string GetCleanText(string text)
        {
            return text.Trim();
        }
    }
}