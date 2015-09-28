using System;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
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

        /// <summary>Hidden questions release</summary>
        private readonly Version version_10 = new Version(10, 0, 0);

        /// <summary>Random value for questionnaire</summary>
        private readonly Version version_11 = new Version(11, 0, 0);

        public Version GetLatestSupportedVersion()
        {
            return version_11;
        }

        public bool IsClientVersionSupported(Version clientVersion)
        {
            var engineVersion = GetLatestSupportedVersion();
            if (engineVersion > clientVersion)
            {
                if (clientVersion < version_5)
                    return false;
            }
            return true;
        }

        public bool IsQuestionnaireDocumentSupportedByClientVersion(QuestionnaireDocument questionnaireDocument, Version clientVersion)
        {
            if (clientVersion < version_10)
            {
                var countOfHiddenQuestions =
                    questionnaireDocument.Find<IQuestion>(q => q.QuestionScope == QuestionScope.Hidden).Count();
             
                return countOfHiddenQuestions == 0;
            }
            return true;
        }
    }
}
