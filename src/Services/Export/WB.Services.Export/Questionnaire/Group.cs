using System;
using System.Collections.Generic;

namespace WB.Services.Export.Questionnaire
{
    public class Group : IQuestionnaireEntity
    {
        public Group()
        {
            Children = Array.Empty<IQuestionnaireEntity>();
            FixedRosterTitles = Array.Empty<FixedRosterTitle>();
        }

        public bool IsRoster { get; set; }

        public Guid? RosterSizeQuestionId { get; set; }

        public bool IsFixedRoster => IsRoster && RosterSizeQuestionId == null;

        public IQuestionnaireEntity Parent { get; private set; }

        public FixedRosterTitle[] FixedRosterTitles { get; set; }
        
        public string VariableName { get; set; }

        public string Title { get; set; }

        public Guid? RosterTitleQuestionId { get; set; }

        public Guid PublicKey { get; set; }

        public IEnumerable<IQuestionnaireEntity> Children { get; set; }

        public IQuestionnaireEntity GetParent()
        {
            return Parent;
        }
    }
}
