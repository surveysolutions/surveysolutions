using System;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services
{
    internal class DesignerEngineVersionService : IDesignerEngineVersionService
    {
        /// <summary>
        /// New Era of c# conditions
        /// </summary>
        private readonly Version version_5 = new Version(5, 0, 0);

        /// <summary>
        /// New service variables which could be used in C# conditions - @rowname, @rowindex, @rowcode, @roster
        /// Custom functions from ZScore( anthropocentric ) and general shortcuts like - InRange, InList ect.
        /// </summary>
        private readonly Version version_6 = new Version(6, 0, 0);

        /// <summary>Functions were extended with IsAnswered function</summary>
        private readonly Version version_7 = new Version(7, 0, 0);

        /// <summary>API used for first release of new Tester (used new RoslynCompile profile)</summary>
        private readonly Version version_8 = new Version(8, 0, 0);
        
        /// <summary>Potatoid release (introduced RosterVector)</summary>
        private readonly Version version_9 = new Version(9, 0, 0);

        /// <summary>Hidden questions and random value release</summary>
        private readonly Version version_10 = new Version(10, 0, 0);

        /// <summary>Yes/No questions</summary>
        private readonly Version version_11 = new Version(11, 0, 0);

        /// <summary>Multiple validation, linked on roster title question, hide questions by condition</summary>
        private readonly Version version_12 = new Version(12, 0, 0);

        /// <summary>
        /// Attachments: Images in static texts.
        /// Filtered linked questions
        /// </summary>
        private readonly Version version_13 = new Version(13, 0, 0);

        /// <summary>Static texts: enablement conditions and validations</summary>
        private readonly Version version_14 = new Version(14, 0, 0);

        /// <summary>Variables</summary>
        private readonly Version version_15 = new Version(15, 0, 0);

        private readonly Version version_16 = new Version(16, 0, 0);


        public Version GetLatestSupportedVersion() => this.version_16;

        public bool IsClientVersionSupported(Version clientVersion)
        { 
            var engineVersion = this.GetLatestSupportedVersion();
            return (clientVersion >= this.version_10 && engineVersion >= clientVersion);
        }

        public bool IsQuestionnaireDocumentSupportedByClientVersion(QuestionnaireDocument questionnaireDocument, Version clientVersion)
        {
            Version questionnaireContentVersion = this.GetQuestionnaireContentVersion(questionnaireDocument);

            if (clientVersion < questionnaireContentVersion)
                return false;

            return true;
        }

        public Version GetQuestionnaireContentVersion(QuestionnaireDocument questionnaireDocument)
        {
            bool hasVariables = questionnaireDocument.Find<Variable>().Any();
            if (hasVariables)
                return version_15;

            bool hasStaticTextsWithConditions = questionnaireDocument.Find<StaticText>(x => x.ValidationConditions.Any() || !string.IsNullOrWhiteSpace(x.ConditionExpression)).Any();
            if (hasStaticTextsWithConditions)
                return version_14;

            var countOfStaticTextsWithAttachment = questionnaireDocument.Find<StaticText>(q => !string.IsNullOrWhiteSpace(q.AttachmentName)).Count();
            if (countOfStaticTextsWithAttachment > 0)
                return version_13;

            var countOfQuestionsWithFilteredLinkedQuestions =
                questionnaireDocument.Find<IQuestion>(q => !string.IsNullOrEmpty(q.LinkedFilterExpression)).Count();
            if (countOfQuestionsWithFilteredLinkedQuestions > 0)
                return version_13;

            var countOfQuestionsWithMultipleValidations = questionnaireDocument.Find<IQuestion>(q => q.ValidationConditions.Count() > 1).Count();
            if (countOfQuestionsWithMultipleValidations > 0)
                return version_12;

            var countOfLinkedOnRosterQuestions = questionnaireDocument.Find<IQuestion>(q => q.LinkedToRosterId.HasValue).Count();
            if (countOfLinkedOnRosterQuestions > 0)
                return version_12;
           
            var countOfYesNoQuestions = questionnaireDocument.Find<IMultyOptionsQuestion>(q => q.YesNoView).Count();
            if (countOfYesNoQuestions > 0)
                return version_11;

            var countOfLookupTables = questionnaireDocument.LookupTables.Count;
            if (countOfLookupTables > 0)
                return version_11;

            return this.version_10;
        }
    }
}
