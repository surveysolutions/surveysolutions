// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AssignSurveyInputModel.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The assign survey input model.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;

namespace Core.Supervisor.Views.Assign
{
    public class AssignSurveyInputModel
    {
        public AssignSurveyInputModel(Guid id, Guid viewerId)
        {
            this.CompleteQuestionnaireId = id;
            this.ViewerId = viewerId;
        }

        public Guid ViewerId { get; set; }
        
        public Guid CompleteQuestionnaireId { get; private set; }
    }
}