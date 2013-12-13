using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Main.Core.View;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

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
            this.IsRoster = group.IsRoster;
            this.RosterSizeQuestionId = group.RosterSizeQuestionId;
            this.RosterSizeSource = group.RosterSizeSource;
            this.RosterFixedTitles = group.RosterFixedTitles;
            this.ConditionExpression = group.ConditionExpression;
            this.Description = group.Description;
            this.Children = @group.Children.Select(composite => new QuestionnaireEntityNode
            {
                Id = composite.PublicKey,
                Type = (composite is IQuestion) ? QuestionnaireEntityType.Question : QuestionnaireEntityType.Group
            }).ToList();
        }

        public int Level { get; set; }

        public string ConditionExpression { get; set; }

        public bool IsRoster { get; set; }

        public Guid? RosterSizeQuestionId { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public RosterSizeSourceType RosterSizeSource { get; set; }

        public string[] RosterFixedTitles { get; set; }

        public string Description { get; set; }

        public Guid Id { get; set; }

        public Guid? ParentId { get; set; }

        public string Title { get; set; }

        public List<QuestionnaireEntityNode> Children { get; set; }
    }
}