﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.QuestionnaireVerification.Services;
using Group = Main.Core.Entities.SubEntities.Group;

namespace WB.Core.SharedKernels.QuestionnaireVerification.Implementation.Services
{
    internal class QuestionnaireUpgradeService : IQuestionnaireUpgradeService
    {
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly int maxLengthOfVariableName = 32;
        public QuestionnaireUpgradeService(IFileSystemAccessor fileSystemAccessor)
        {
            this.fileSystemAccessor = fileSystemAccessor;
        }

        public QuestionnaireDocument CreateRostersVariableName(QuestionnaireDocument originalDocument)
        {
            var document = originalDocument.Clone();
            var missingVariableNames = this.GetMissingVariableNames(document);
            foreach (var missingVariableName in missingVariableNames)
            {
                missingVariableName.Value.VariableName = missingVariableName.Key;
            }
            return document;
        }

        public Dictionary<string, IGroup> GetMissingVariableNames(QuestionnaireDocument originalDocument)
        {
            var result = new Dictionary<string, IGroup>();
            var documentVariableName = this.fileSystemAccessor.MakeValidFileName(originalDocument.Title);
            var rosters = originalDocument.Find<Group>(g => g.IsRoster || g.Propagated != Propagate.None);
            var rostersVariableNames = new HashSet<string> { documentVariableName };

            foreach (var roster in rosters)
            {
                if (this.IsRosterVariableNameValid(roster.VariableName, rostersVariableNames))
                {
                    rostersVariableNames.Add(roster.VariableName);
                    continue;
                }
                var originalVariableName = this.CreateValidVariableNameFormGroupTitle(roster);
                var variableName = originalVariableName;

                int i = 1;
                while (rostersVariableNames.Contains(variableName))
                {
                    variableName = originalVariableName + i;
                    if (variableName.Length > this.maxLengthOfVariableName)
                    {
                        variableName = new string(originalVariableName.Take(this.maxLengthOfVariableName - i.ToString(CultureInfo.InvariantCulture).Length).ToArray()) + i;
                    }
                    i++;
                }
                rostersVariableNames.Add(variableName);
                result.Add(variableName, roster);
            }
            return result;
        }

        private string CreateValidVariableNameFormGroupTitle(IGroup roster)
        {
            string title = string.IsNullOrEmpty(roster.VariableName) ? roster.Title : roster.VariableName;
            var invalidCharactersRegEx = new Regex("[^a-zA-Z0-9_]");
            var variableName = invalidCharactersRegEx.Replace(title, "").ToLowerInvariant();
            while (!string.IsNullOrEmpty(variableName))
            {
                if (Char.IsNumber(variableName[0]))
                    variableName = variableName.Substring(1, variableName.Length - 1);
                else
                    break;
            }
            if (string.IsNullOrEmpty(variableName))
                return "roster";
            if (variableName.Length <= this.maxLengthOfVariableName)
                return variableName;
            return new string(variableName.Take(this.maxLengthOfVariableName).ToArray());
        }

        private bool IsRosterVariableNameValid(string rosterVariableName, HashSet<string> existingVariableNames)
        {
            if (string.IsNullOrEmpty(rosterVariableName))
                return false;
            if (rosterVariableName.Length > this.maxLengthOfVariableName)
                return false;
            var validVariableNameRegEx = new Regex("^[_A-Za-z][_A-Za-z0-9]*$");
            if (!validVariableNameRegEx.IsMatch(rosterVariableName))
                return false;
            return !existingVariableNames.Contains(rosterVariableName);
        }
    }
}
