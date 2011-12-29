using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Commands
{
    public class UpdateQuestionnaireCommand : ICommand
    {
        public string QuestionnaireId { get; set; }

        public string Title{get; set;}
        public UserLight Executor { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="questionnaireId"></param>
        /// <param name="title"></param>
        /// <param name="executor"></param>
        public UpdateQuestionnaireCommand(string questionnaireId, string title, UserLight executor)
        {
            this.QuestionnaireId = IdUtil.CreateQuestionnaireId(questionnaireId);
            this.Title = title;
            this.Executor = executor;
        }

        
    }
}
