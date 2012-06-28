using System;
using System.Net;
using System.Web;
using Newtonsoft.Json;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Commands.Questionnaire.Completed
{
    public class CreateNewCompleteQuestionnaireCommand : ICommand
    {
        public string QuestionnaireId { get; private set; }
        public UserLight Creator { get; private set; }
        public Guid CompleteQuestionnaireGuid { get;private set; }
        public SurveyStatus Status { private set; get; }
        public UserLight Executor { get; set; }

        [JsonConstructor]
        public CreateNewCompleteQuestionnaireCommand(string questionnaireId, 
            Guid completeQuestionnaireGuid,
            UserLight creator, 
            SurveyStatus status,
            UserLight executor)
        {
            if(string.IsNullOrEmpty(questionnaireId))
                throw  new HttpException(404, "QuestionnaireId can't be null");
            if (string.IsNullOrEmpty(creator.Id))
                throw new HttpException(404, "User id can't be null");
            this.QuestionnaireId = IdUtil.CreateQuestionnaireId(IdUtil.ParseId(questionnaireId));
            this.CompleteQuestionnaireGuid = completeQuestionnaireGuid;
            this.Creator = creator;
            this.Status = status;
            this.Executor = executor;
        }
    }
}
