﻿using System;
using System.Collections.Generic;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Snapshots
{
    public class QuestionnaireState
    {
        public QuestionnaireState(QuestionnaireDocument document, Dictionary<Guid, IQuestion> questionCache, Dictionary<Guid, IGroup> groupCache,
            bool isProxyToPlainQuestionnaireRepository)
        {
            this.Document = document;
            this.QuestionCache = questionCache;
            this.GroupCache = groupCache;
            this.IsProxyToPlainQuestionnaireRepository = isProxyToPlainQuestionnaireRepository;
        }

        public QuestionnaireDocument Document { get; private set; }
        public Dictionary<Guid, IQuestion> QuestionCache { get; private set; }
        public Dictionary<Guid, IGroup> GroupCache { get; private set; }
        public bool IsProxyToPlainQuestionnaireRepository { get; private set; }
    }
}
