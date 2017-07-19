using Main.Core.Entities.SubEntities;

namespace WB.UI.Headquarters.Models.User
{
    public class ResponsibleComboboxOptionModel
    {
        public ResponsibleComboboxOptionModel(string key, string value, UserRoles role)
        {
            this.Key = key;
            this.Value = value;
            this.IconClass = role.ToString().ToLower();
        }

        public string Key { get; set; }
        public string Value { get; set; }
        public string IconClass { get; set; }
    }
}