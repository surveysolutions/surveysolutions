using Android.Content;
using Android.Util;

namespace WB.UI.QuestionnaireTester.CustomControls.MvxBindableAutoCompleteTextViewControl
{
    public static class MvxBindableListViewHelpers
    {
        public static int ReadAttributeValue(Context context, IAttributeSet attrs, int[] groupId, int requiredAttributeId)
        {
            var typedArray = context.ObtainStyledAttributes(attrs, groupId);

            try
            {
                var numStyles = typedArray.IndexCount;
                for (var i = 0; i < numStyles; ++i)
                {
                    var attributeId = typedArray.GetIndex(i);
                    if (attributeId == requiredAttributeId)
                    {
                        return typedArray.GetResourceId(attributeId, 0);
                    }
                }
                return 0;
            }
            finally
            {
                typedArray.Recycle();
            }
        }
    }
}