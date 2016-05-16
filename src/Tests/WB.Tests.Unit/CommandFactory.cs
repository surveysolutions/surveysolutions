using System;
using System.Collections.Generic;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.LookupTables;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Macros;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.StaticText;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Core.SharedKernels.SurveyManagement.Commands;

namespace WB.Tests.Unit
{
    internal class CommandFactory
    {
        public AddLookupTable AddLookupTable(Guid questionnaireId, Guid lookupTableId, Guid responsibleId, string lookupTableName = "table")
            => new AddLookupTable(questionnaireId, lookupTableName, null, lookupTableId, responsibleId);

        public AddMacro AddMacro(Guid questionnaire, Guid? macroId = null, Guid? userId = null)
            => new AddMacro(questionnaire, macroId ?? Guid.NewGuid(), userId ?? Guid.NewGuid());

        public AnswerYesNoQuestion AnswerYesNoQuestion(Guid? userId = null,
            Guid? questionId = null, RosterVector rosterVector = null, AnsweredYesNoOption[] answeredOptions = null,
            DateTime? answerTime = null)
            => new AnswerYesNoQuestion(
                interviewId: Guid.NewGuid(),
                userId: userId ?? Guid.NewGuid(),
                questionId: questionId ?? Guid.NewGuid(),
                rosterVector: rosterVector ?? Core.SharedKernels.DataCollection.RosterVector.Empty,
                answerTime: answerTime ?? DateTime.UtcNow,
                answeredOptions: answeredOptions ?? new AnsweredYesNoOption[] { });

        public CloneQuestionnaire CloneQuestionnaire(
            QuestionnaireIdentity questionnaireIdentity = null,
            Guid? questionnaireId = null, long? questionnaireVersion = null,
            string newTitle = null)
            => new CloneQuestionnaire(
                questionnaireIdentity?.QuestionnaireId ?? questionnaireId ?? Guid.NewGuid(),
                questionnaireIdentity?.Version ?? questionnaireVersion ?? 777,
                newTitle ?? "New Title of Cloned Copy",
                Guid.NewGuid());

        public DeleteLookupTable DeleteLookupTable(Guid questionnaireId, Guid lookupTableId, Guid responsibleId)
            => new DeleteLookupTable(questionnaireId, lookupTableId, responsibleId);

        public DeleteMacro DeleteMacro(Guid questionnaire, Guid? macroId = null, Guid? userId = null)
            => new DeleteMacro(questionnaire, macroId ?? Guid.NewGuid(), userId ?? Guid.NewGuid());

        public ImportFromDesigner ImportFromDesigner(Guid? questionnaireId = null, string title = "Questionnaire X",
            Guid? responsibleId = null, string base64StringOfAssembly = "<base64>assembly</base64> :)",
            long questionnaireContentVersion = 1)
            => new ImportFromDesigner(
                responsibleId ?? Guid.NewGuid(),
                new QuestionnaireDocument
                {
                    PublicKey = questionnaireId ?? Guid.NewGuid(),
                    Title = title,
                },
                false,
                base64StringOfAssembly,
                questionnaireContentVersion);

        public LinkUserToDevice LinkUserToDeviceCommand(Guid userId, string deviceId)
            => new LinkUserToDevice(userId, deviceId);

        public UpdateLookupTable UpdateLookupTable(Guid questionnaireId, Guid lookupTableId, Guid responsibleId, string lookupTableName = "table")
            => new UpdateLookupTable(questionnaireId, lookupTableId, responsibleId, lookupTableName, "file");

        public UpdateMacro UpdateMacro(Guid questionnaireId, Guid macroId, string name, string content, string description, Guid? userId)
            => new UpdateMacro(questionnaireId, macroId, name, content, description, userId ?? Guid.NewGuid());

        public UpdateStaticText UpdateStaticText(Guid questionnaireId, Guid entityId, string text, string attachmentName, Guid responsibleId,
            string enablementCondition, bool hideIfDisabled = false, IList<ValidationCondition> validationConditions = null)
            => new UpdateStaticText(questionnaireId, entityId, text, attachmentName, responsibleId, enablementCondition, hideIfDisabled, validationConditions);
    }
}