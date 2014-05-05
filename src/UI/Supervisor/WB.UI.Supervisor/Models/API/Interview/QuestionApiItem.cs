using System.Runtime.Serialization;

namespace WB.UI.Supervisor.Models.API.Interview
{
    public class QuestionApiItem
    {
        public QuestionApiItem(string name, object value)
        {
            this.Value = value;
            this.Name = name;
        }

        [DataMember]
        public string Name { set; get; }
        [DataMember]
        public object Value { set; get; }
    }
}