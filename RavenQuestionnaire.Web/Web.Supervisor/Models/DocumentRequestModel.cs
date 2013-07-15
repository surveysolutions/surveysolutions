namespace Web.Supervisor.Models
{
    using System;

    public class DocumentRequestModel
    {
        #region Public Properties

        public Guid? TemplateId { get; set; }
        public Guid? ResponsibleId { get; set; }
        public Guid? StatusId { get; set; }
        public bool OnlyAssigned { get; set; }

        #endregion
    }
}