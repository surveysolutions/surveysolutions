using System;
using System.IO;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using RavenQuestionnaire.Core.Domain;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Commands.Questionnaire
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof(QuestionnaireAR), "UploadImage")]
    public class UploadImageCommand : CommandBase
    {
        public UploadImageCommand(Guid publicKey, Guid questionnaireId, string title, string description, Guid imagePublicKey)
        {
            PublicKey = publicKey;
            QuestionnaireId = questionnaireId;
            Description = description;
            Title = title;
            ImagePublicKey = imagePublicKey;
        }
      


        #region Properties

        public Guid PublicKey { get; set; }
        [AggregateRootId]
        public Guid QuestionnaireId { get; set; }

        public string Title { get; private set; }

        public string Description { get; private set; }

        public Guid ImagePublicKey { get; private set; }

        #endregion
    }
}