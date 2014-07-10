using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Main.Core.Documents;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.QuestionnaireUpgrader.Services;
using Group = Main.Core.Entities.SubEntities.Group;

namespace WB.Core.SharedKernels.QuestionnaireUpgrader.Implementation.Services
{
    internal class QuestionnaireUpgradeService : IQuestionnaireUpgradeService
    {
        private readonly IFileSystemAccessor fileSystemAccessor;
    //    private readonly Regex validVariableNameRegEx = new Regex("^[_A-Za-z][_A-Za-z0-9]*$");

        public QuestionnaireUpgradeService(IFileSystemAccessor fileSystemAccessor)
        {
            this.fileSystemAccessor = fileSystemAccessor;
        }

        public QuestionnaireDocument CreateRostersVariableName(QuestionnaireDocument originalDocument)
        {
            var document = originalDocument.Clone();
            var rosters = document.Find<Group>(g => g.IsRoster);
            var rostersVariableNames = new HashSet<string> { this.fileSystemAccessor.MakeValidFileName(originalDocument.Title) };

            foreach (var roster in rosters)
            {
                if (IsRosterVariableNameValid(roster.VariableName, rostersVariableNames))
                {
                    rostersVariableNames.Add(roster.VariableName);
                    continue;
                }
                var originalVariableName =CreateValidVariableNameFormGroupTitle(roster.Title);
                var variableName = originalVariableName;

                int i = 1;
                while (rostersVariableNames.Contains(variableName))
                {
                    variableName = originalVariableName + i;
                    if (variableName.Length > 32)
                    {
                        variableName = new string(originalVariableName.Take(originalVariableName.Length - i.ToString(CultureInfo.InvariantCulture).Length).ToArray()) + i;
                    }
                    i++;
                }
                rostersVariableNames.Add(variableName);
                roster.VariableName = variableName;
            }
            return document;
        }

        private string CreateValidVariableNameFormGroupTitle(string title)
        {
            var invalidCharactersRegEx = new Regex("[^a-zA-Z0-9_]");
            var variableName = invalidCharactersRegEx.Replace(title, "").ToLowerInvariant();
            if (string.IsNullOrEmpty(variableName))
                return "a";
            if (variableName.Length <= 32)
                return variableName;
            return new string(variableName.Take(32).ToArray());
        }

        private bool IsRosterVariableNameValid(string rosterVariableName, HashSet<string> existingVariableNames)
        {
            if (string.IsNullOrEmpty(rosterVariableName))
                return false;
            if (rosterVariableName.Length > 32)
                return false;
            var validVariableNameRegEx = new Regex("^[_A-Za-z][_A-Za-z0-9]*$");
            if (!validVariableNameRegEx.IsMatch(rosterVariableName))
                return false;
            return !existingVariableNames.Contains(rosterVariableName);
        }
    }
}
