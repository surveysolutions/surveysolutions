// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuestionDeleted.cs" company="">
//   
// </copyright>
// <summary>
//   The question deleted.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Main.Core.Events.Questionnaire
{
    using System;

    using Ncqrs.Eventing.Storage;

    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:QuestionDeleted")]
    public class QuestionDeleted
    {
        public QuestionDeleted(Guid questionId)
        {
            this.QuestionId = questionId;
        }

        [Obsolete]
        public QuestionDeleted(Guid questionId, Guid parentPublicKey)
            : this(questionId)
        {
            this.ParentPublicKey = parentPublicKey;
        }

        public Guid QuestionId { get; set; }

        [Obsolete]
        public Guid ParentPublicKey { get; set; }
    }
}