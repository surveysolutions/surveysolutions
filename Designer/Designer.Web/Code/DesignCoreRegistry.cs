using Main.Core;

namespace Designer.Web
{
    public class DesignCoreRegistry : CoreRegistry
    {
        public DesignCoreRegistry(string repositoryPath, bool isEmbeded)
            : base(repositoryPath, isEmbeded)
        {
 
        }

        public override void Load()
        {
            base.Load();
        }
    }
}