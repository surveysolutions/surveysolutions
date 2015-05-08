using System;
using System.Collections.Generic;

using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    public interface IStatefulInterview
    {
        string QuestionnaireId { get; set; }
        long QuestionnaireVersion { get; set; }
        Guid Id { get; set; }
        Dictionary<string, BaseInterviewAnswer> Answers { get; }
        Dictionary<string, InterviewGroup> Groups { get; }
        Dictionary<string, List<string>> RosterInstancesIds { get; }
        
        bool HasErrors { get; set; }
        bool IsInProgress { get; set; }

        GpsCoordinatesAnswer GetGpsCoordinatesAnswerModel(Identity identity);

        DateTimeAnswer GetDateTimeAnswerModel(Identity identity);

        MultimediaAnswer GetMultimediaAnswerModel(Identity identity);

        QrBarcodeAnswer GetQrBarcodeAnswerModel(Identity identity);

        TextListAnswer GetTextListAnswerModel(Identity identity);

        LinkedSingleOptionAnswer GetLinkedSingleOptionAnswerModel(Identity identity);

        MultiOptionAnswer GetMultiOptionAnswerModel(Identity identity);

        LinkedMultiOptionAnswer GetLinkedMultiOptionAnswerModel(Identity identity);

        IntegerNumericAnswer GetIntegerNumericAnswerModel(Identity identity);

        RealNumericAnswer GetRealNumericAnswerModel(Identity identity);

        MaskedTextAnswer GetTextAnswerModel(Identity identity);

        SingleOptionAnswer GetSingleOptionAnswerModel(Identity identity);
        
        bool IsValid(Identity identity);

        bool IsEnabled(Identity entityIdentity);
    }
}