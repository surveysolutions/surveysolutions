using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Main.Core.View;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace WB.UI.Designer.Views.Questionnaire
{
    public enum QuestionnaireEntityType
    {
        Question = 1,
        Group = 10
    }

    public class QuestionnaireEntityNode
    {
        public QuestionnaireEntityType Type { get; set; }
        public Guid Id { get; set; }
    }

    public class GroupView : ICompositeView
    {
        public GroupView(IGroup group, Guid? parentId, int level)
        {
            this.Id = group.PublicKey;
            this.ParentId = parentId;
            this.Level = level;
            this.Title = group.Title;
            this.Propagated = group.Propagated;
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

        [JsonConverter(typeof (StringEnumConverter))]
        public Propagate Propagated { get; set; }

        public string Description { get; set; }

        public Guid Id { get; set; }

        public Guid? ParentId { get; set; }

        public string Title { get; set; }

        public List<QuestionnaireEntityNode> Children { get; set; }
    }
}