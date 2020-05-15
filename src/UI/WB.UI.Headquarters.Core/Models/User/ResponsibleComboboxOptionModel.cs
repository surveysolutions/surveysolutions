using System;

namespace WB.UI.Headquarters.Models.User
{
    public class ResponsibleComboboxOptionModel
    {
        public ResponsibleComboboxOptionModel(string key, string value, string iconClass, Guid? supervisorId)
        {
            this.Key = key;
            this.Value = value;
            this.IconClass = iconClass;
            this.SupervisorId = supervisorId;
        }

        public string Key { get; set; }
        public string Value { get; set; }
        public string IconClass { get; set; }
        public Guid? SupervisorId { get; }
    }
}
