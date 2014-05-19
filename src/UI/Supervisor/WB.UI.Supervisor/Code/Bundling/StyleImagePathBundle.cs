using System.Web.Optimization;

namespace WB.UI.Supervisor.Code.Bundling
{
    public class StyleImagePathBundle : Bundle
    {
        public StyleImagePathBundle(string virtualPath)
            : base(virtualPath)
        {
            this.RegisterTransformers();
        }

        public StyleImagePathBundle(string virtualPath, string cdnPath)
            : base(virtualPath, cdnPath) 
        {
            this.RegisterTransformers();
        }

        private void RegisterTransformers()
        {
            base.Transforms.Add(new StyleRelativePathTransform());
            base.Transforms.Add(new CssMinify());
        }
    }
}