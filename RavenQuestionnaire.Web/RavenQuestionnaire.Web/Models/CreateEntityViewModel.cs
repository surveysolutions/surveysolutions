using System.Web.Mvc.Ajax;

namespace RavenQuestionnaire.Web.Models
{
    public class CreateEntityViewModel<T>
    {
        public T Model { get; set; }
        public InsertionMode InsertionMode { get; set; }
        public string TargetId { get; set; }
        public bool IsNew { get; private set; }

        public CreateEntityViewModel(T defaultModel, string targetId)
        {
            Model = defaultModel;
            InsertionMode= InsertionMode.Replace;
            TargetId = targetId;
            IsNew = false;
        }
        public CreateEntityViewModel(T model, string targetId, bool isNew)
        {
            Model = model;
            InsertionMode = isNew ? InsertionMode.InsertAfter : InsertionMode.Replace;
            TargetId = targetId;
            IsNew = isNew;
        }
    }
}