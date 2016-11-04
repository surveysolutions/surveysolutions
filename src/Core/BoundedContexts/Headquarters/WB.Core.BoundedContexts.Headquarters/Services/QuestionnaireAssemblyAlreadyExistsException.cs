using System;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Services
{
    public class QuestionnaireAssemblyAlreadyExistsException : Exception
    {
        public QuestionnaireIdentity QuestionnaireIdentity { get; set; }

        public QuestionnaireAssemblyAlreadyExistsException(string message, QuestionnaireIdentity questionnaireIdentity) : base(message)
        {
            this.QuestionnaireIdentity = questionnaireIdentity;
        }
    }
}