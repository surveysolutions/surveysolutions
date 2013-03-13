// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuestionnaireDeleted.cs" company="The World Bank">
//   The World Bank
// </copyright>
// <summary>
//   TODO: Delete summary.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Main.Core.Events.Questionnaire
{
    using System;

    using Ncqrs.Eventing.Storage;

    /// <summary>
    /// TODO: Delete summary.
    /// </summary>
    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:QuestionnaireDeleted")]
    public class QuestionnaireDeleted
    {
    }
}