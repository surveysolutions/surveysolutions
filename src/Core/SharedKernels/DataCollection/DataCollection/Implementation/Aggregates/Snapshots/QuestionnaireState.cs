﻿using System;
using System.Collections.Generic;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection.Aggregates;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Snapshots
{
    public class QuestionnaireState
    {
        public QuestionnaireState(bool isProxyToPlainQuestionnaireRepository, Dictionary<long, IQuestionnaire> availableVersions)
        {
            this.AvailableVersions = availableVersions;
            this.IsProxyToPlainQuestionnaireRepository = isProxyToPlainQuestionnaireRepository;
        }
        public bool IsProxyToPlainQuestionnaireRepository { get; private set; }
        public Dictionary<long, IQuestionnaire> AvailableVersions { get; private set; }
    }
}
