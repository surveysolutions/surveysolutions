namespace WB.Core.GenericSubdomains.Utils
{
    public class Unit
    {
        private Unit() { }

        private static readonly Unit empty = new Unit();

        public static Unit Empty
        {
            get { return empty; }
        }

        public override string ToString()
        {
            return "nothing";
        }
    }
}