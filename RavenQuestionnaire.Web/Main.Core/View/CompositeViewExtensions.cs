namespace Main.Core.View
{
    public static class CompositeViewExtensions
    {
        public static int GetDepthIn(this ICompositeView view)
        {
            int result = 0;
            var parent = view.ParentView;
            while (parent != null)
            {
                result++;
                parent = parent.ParentView;
            }

            return result;
        }
    }
}