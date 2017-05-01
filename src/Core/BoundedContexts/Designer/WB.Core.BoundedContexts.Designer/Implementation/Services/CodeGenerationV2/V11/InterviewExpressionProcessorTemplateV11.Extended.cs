using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGenerationV2.V11
{
    public partial class InterviewExpressionProcessorTemplateV11
    {
        public InterviewExpressionProcessorTemplateV11(CodeGenerationModel model)
        {
            this.Model = model;
            this.Ids = new Dictionary<Guid, string>
            {
                {this.Model.Id, CodeGeneratorV2.QuestionnaireIdName}
            };

            foreach (var question in this.Model.AllQuestions)
            {
                this.Ids.Add(question.Id, question.Variable);
            }

            foreach (var roster in this.Model.AllRosters)
            {
                this.Ids.Add(roster.Level.Id, roster.Variable);
            }

            foreach (var staticText in this.Model.AllStaticTexts)
            {
                this.Ids.Add(staticText.Id, staticText.Variable);
            }
        }

        public CodeGenerationModel Model { get; private set; }
        public Dictionary<Guid, string> Ids { get; private set; }

        protected LevelTemplateV11 CreateLevelTemplate(LevelModel level, CodeGenerationModel model)
        {
            return new LevelTemplateV11(level, model);
        }
    }
}