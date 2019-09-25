using System;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire
{
    public class ImportQuestionnaireToHq : QuestionnaireCommand
    {
        public ImportQuestionnaireToHq(Guid responsibleId, string site, string ipAddress, QuestionnaireDocument source)
            : base(source.PublicKey, responsibleId)
        {
            IpAddress = ipAddress;
            Source = source;
            Site = site;
        }

        public string Site { get; }
        public string IpAddress { get; }
        public QuestionnaireDocument Source { get; }
    }
}
