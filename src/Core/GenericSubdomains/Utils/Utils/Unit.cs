namespace WB.Core.GenericSubdomains.Utils
{
    /// <summary>
    /// The unit type is the terminal object in the category of types and typed functions.
    /// It should not be confused with the zero or bottom type, which allows no values and is the initial object in this category.
    /// http://en.wikipedia.org/wiki/Unit_type
    /// </summary>
    public class Unit
    {
        private Unit() { }

        private static readonly Unit value = new Unit();

        public static Unit Value
        {
            get { return value; }
        }

        public override string ToString()
        {
            return "nothing";
        }
    }
}