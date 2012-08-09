using System;
using System.Text;

namespace RavenQuestionnaire.Core.Views.Assignment
{
    public class AssigmentViewInputModel
    {
        public string Id { get; set; }

        public AssigmentViewInputModel(string Id)
        {
            this.Id = Id;
        }
    }
}
