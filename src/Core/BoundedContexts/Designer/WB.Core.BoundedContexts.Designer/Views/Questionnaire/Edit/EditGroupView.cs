using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit
{
    public class EditGroupView : ICompositeView
    {
        public EditGroupView(IGroup group, Guid? parentId, int level)
        {
            this.Id = group.PublicKey;
            this.ParentId = parentId;
            this.Level = level;
            this.Title = group.Title;
            this.VariableName = group.VariableName;
            this.IsRoster = group.IsRoster;
            this.RosterSizeQuestionId = group.RosterSizeQuestionId;
            this.RosterSizeSource = group.RosterSizeSource;
            this.FixedRosterTitles = group.FixedRosterTitles;
            this.RosterTitleQuestionId = group.RosterTitleQuestionId;
            this.ConditionExpression = group.ConditionExpression;
            this.Description = group.Description;
            this.Children = @group.Children.Select(composite => new QuestionnaireEntityNode
            {
                Id = composite.PublicKey,
                Type =
                    (composite is IQuestion)
                        ? QuestionnaireEntityType.Question
                        : ((composite is IStaticText)
                            ? QuestionnaireEntityType.StaticText
                            : QuestionnaireEntityType.Group)
            }).ToList();
        }

        public int Level { get; set; }

        public string ConditionExpression { get; set; }

        public bool IsRoster { get; set; }

        public Guid? RosterSizeQuestionId { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public RosterSizeSourceType RosterSizeSource { get; set; }

        public FixedRosterTitle[] FixedRosterTitles { get; set; }
        public Guid? RosterTitleQuestionId { get; set; }

        public string Description { get; set; }

        public Guid Id { get; set; }

        public Guid? ParentId { get; set; }

        public string Title { get; set; }

        public string VariableName { get; set; }

        public List<QuestionnaireEntityNode> Children { get; set; }
    }
}