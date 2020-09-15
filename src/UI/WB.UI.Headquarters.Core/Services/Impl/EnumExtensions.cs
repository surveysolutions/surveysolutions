using System;
using WB.UI.Headquarters.Resources;

namespace WB.UI.Headquarters.Services.Impl
{
    public static class EnumExtensions
    {
        public static string ToUiString(this Enum val)
        {
            Type enumType = val.GetType();
            string stringValue = Enum.GetName(enumType, val);
            return EnumNames.ResourceManager.GetString($"{enumType.Name}_{stringValue}", EnumNames.Culture);
        }
    }
}
