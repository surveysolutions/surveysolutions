﻿using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation.ServiceVariables;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Views
{
    public class QuestionnaireExportStructure
    {
        private QuestionnaireIdentity identity;
        private int? maxRosterDepthInQuestionnaire = null;
        

        public QuestionnaireExportStructure()
        {
            this.HeaderToLevelMap = new Dictionary<ValueVector<Guid>, HeaderStructureForLevel>();
        }

        public Guid QuestionnaireId { get; set; }
        public Dictionary<ValueVector<Guid>, HeaderStructureForLevel> HeaderToLevelMap { get; set; }
        public long Version { get; set; }

        public QuestionnaireIdentity Identity => identity ?? (identity = new QuestionnaireIdentity(QuestionnaireId, Version));

        public IEnumerable<string> GetAllParentColumnNamesForLevel(ValueVector<Guid> levelScopeVector)
        {
            for (int i = 1; i < levelScopeVector.Length; i++)
            {
                var parentLevelScopeVector = ValueVector.Create(levelScopeVector.Take(levelScopeVector.Length - i).ToArray());
                string parentLevelName = this.HeaderToLevelMap.GetOrNull(parentLevelScopeVector)?.LevelName
                                         ?? $"{ServiceColumns.ParentId}{i + 1}";
                yield return $"{parentLevelName}__id";
            }

            if (levelScopeVector.Length != 0)
            {
                yield return ServiceColumns.InterviewId;
                yield return ServiceColumns.Key;
            }
        }

        private ILookup<Guid, IExportedHeaderItem> headerToQuestionMap;
        public IExportedHeaderItem GetExportedQuestionHeaderItemForQuestion(Guid id)
        {
            if (this.headerToQuestionMap == null)
            {
                this.headerToQuestionMap = 
                    HeaderToLevelMap.SelectMany(h => h.Value.HeaderItems).ToLookup(l => l.Key, l => l.Value);
            }
            
            return this.headerToQuestionMap[id].FirstOrDefault();
        }

        public int MaxRosterDepth
        {
            get
            {
                if (!maxRosterDepthInQuestionnaire.HasValue)
                {
                    maxRosterDepthInQuestionnaire = this.HeaderToLevelMap.Values.Max(x => x.LevelScopeVector.Count);
                }

                return maxRosterDepthInQuestionnaire.Value;
            }
        }
    }
}
