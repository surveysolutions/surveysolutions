using System;
using Main.Core.Documents;
using Ncqrs.Commanding;

namespace WB.Core.Questionnaire.ImportService.Commands
{
    public class ImportQuestionnaireCommand : CommandBase
    {
        public ImportQuestionnaireCommand(Guid createdBy, IQuestionnaireDocument source)
            : base(source.PublicKey)
        {
            CreatedBy = createdBy;
            Source = source;
        }

        /// <summary>
        ///     Gets or sets the created by.
        /// </summary>
        public Guid CreatedBy { get; private set; }
        /// <summary>
        /// Gets or sets the source.
        /// </summary>
        public IQuestionnaireDocument Source { get; set; }
    }
}
