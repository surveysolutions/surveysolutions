using System;
using System.Collections.Generic;

using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Aggregates
{
    public interface IStatefulInterview
    {
        string QuestionnaireId { get; set; }
        long QuestionnaireVersion { get; set; }
        Guid Id { get; set; }
        IReadOnlyDictionary<string, BaseInterviewAnswer> Answers { get; }
        IReadOnlyDictionary<string, InterviewGroup> Groups { get; }
        IReadOnlyDictionary<string, List<Identity>> RosterInstancesIds { get; }
        
        bool HasErrors { get; set; }
        bool IsInProgress { get; set; }

        GpsCoordinatesAnswer GetGpsCoordinatesAnswer(Identity identity);

        DateTimeAnswer GetDateTimeAnswer(Identity identity);

        MultimediaAnswer GetMultimediaAnswer(Identity identity);

        QRBarcodeAnswer GetQRBarcodeAnswer(Identity identity);

        TextListAnswer GetTextListAnswer(Identity identity);

        LinkedSingleOptionAnswer GetLinkedSingleOptionAnswer(Identity identity);

        MultiOptionAnswer GetMultiOptionAnswer(Identity identity);

        LinkedMultiOptionAnswer GetLinkedMultiOptionAnswer(Identity identity);

        IntegerNumericAnswer GetIntegerNumericAnswer(Identity identity);

        RealNumericAnswer GetRealNumericAnswer(Identity identity);

        MaskedTextAnswer GetTextAnswer(Identity identity);

        SingleOptionAnswer GetSingleOptionAnswer(Identity identity);
        
        bool IsValid(Identity identity);

        bool IsEnabled(Identity entityIdentity);

        bool WasAnswered(Identity entityIdentity);

        string GetInterviewerAnswerComment(Identity entityIdentity);

        /// <summary>
        /// Gets string representation of an answer. 
        /// </summary>
        /// <returns>null if question is not answered yet.</returns>
        BaseInterviewAnswer FindBaseAnswerByOrDeeperRosterLevel(Guid questionId, decimal[] targetRosterVector);

        InterviewRoster FindRosterByOrDeeperRosterLevel(Guid rosterId, decimal[] targetRosterVector);
    }
}