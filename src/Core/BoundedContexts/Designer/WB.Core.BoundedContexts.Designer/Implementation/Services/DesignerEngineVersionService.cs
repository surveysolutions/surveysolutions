using System;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Services;

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


        public Version GetLatestSupportedVersion()
        {
            return version_12;
        }

        public bool IsClientVersionSupported(Version clientVersion)
        {
            var engineVersion = GetLatestSupportedVersion();
            if (engineVersion > clientVersion)
            {
                if (clientVersion < version_9)
                    return false;
            }
            return true;
        }

        public bool IsQuestionnaireDocumentSupportedByClientVersion(QuestionnaireDocument questionnaireDocument, Version clientVersion)
        {
            var questionnaireContentVersion = this.GetQuestionnaireContentVersion(questionnaireDocument);
            if (clientVersion < this.version_12 && questionnaireContentVersion == this.version_12)
                return false;

            if (clientVersion < this.version_11 && questionnaireContentVersion == this.version_11)
                return false;

            if (clientVersion == this.version_9 && questionnaireContentVersion == this.version_10)
                return false;

            return true;
        }

        public Version GetQuestionnaireContentVersion(QuestionnaireDocument questionnaireDocument)
        {
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

            var countOfHiddenQuestions = questionnaireDocument.Find<IQuestion>(q => q.QuestionScope == QuestionScope.Hidden).Count();
            if (countOfHiddenQuestions > 0)
                return version_10;

            return this.version_9;
        }
    }
}
