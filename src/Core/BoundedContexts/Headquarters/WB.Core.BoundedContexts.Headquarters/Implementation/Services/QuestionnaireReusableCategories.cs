using System;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services
{
    public class QuestionnaireReusableCategories
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public string Text { get; set; }
    }
}
