﻿using System;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Assignments
{
    public interface IAssignmentFactory
    {
        Assignment CreateAssignment(QuestionnaireIdentity questionnaireId, Guid responsibleId,
            int? quantity, string email, string password, bool? webMode, bool? isAudioRecordingEnabled,
            string comments);
    }

    class AssignmentFactory : IAssignmentFactory
    {
        private readonly IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaires;

        public AssignmentFactory(IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaires)
        {
            this.questionnaires = questionnaires;
        }

        public Assignment CreateAssignment(QuestionnaireIdentity questionnaireId, Guid responsibleId,
            int? quantity, string email, string password, bool? webMode, bool? isAudioRecordingEnabled,
            string comments)
        {
            bool isAudioRecordingEnabledValue = isAudioRecordingEnabled ?? this.questionnaires.Query(_ => _
                .Where(q => q.Id == questionnaireId.ToString())
                .Select(q => q.IsAudioRecordingEnabled).FirstOrDefault());

            return new Assignment(questionnaireId, responsibleId, quantity, isAudioRecordingEnabledValue, email, password, webMode, comments);
        }
    }
}
