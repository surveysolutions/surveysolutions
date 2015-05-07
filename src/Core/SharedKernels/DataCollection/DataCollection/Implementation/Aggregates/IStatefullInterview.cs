using System;
using System.Collections.Generic;

using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    public interface IStatefullInterview
    {
        string QuestionnaireId { get; set; }
        long QuestionnaireVersion { get; set; }
        Guid Id { get; set; }
        Dictionary<string, AbstractInterviewAnswerModel> Answers { get; set; }
        Dictionary<string, InterviewGroupModel> Groups { get; set; }
        Dictionary<string, List<string>> RosterInstancesIds { get; set; }
        
        bool HasErrors { get; set; }
        bool IsInProgress { get; set; }

        GpsCoordinatesAnswerModel GetGpsCoordinatesAnswerModel(Identity identity);

        DateTimeAnswerModel GetDateTimeAnswerModel(Identity identity);

        MultimediaAnswerModel GetMultimediaAnswerModel(Identity identity);

        QrBarcodeAnswerModel GetQrBarcodeAnswerModel(Identity identity);

        TextListAnswerModel GetTextListAnswerModel(Identity identity);

        LinkedSingleOptionAnswerModel GetLinkedSingleOptionAnswerModel(Identity identity);

        MultiOptionAnswerModel GetMultiOptionAnswerModel(Identity identity);

        LinkedMultiOptionAnswerModel GetLinkedMultiOptionAnswerModel(Identity identity);

        IntegerNumericAnswerModel GetIntegerNumericAnswerModel(Identity identity);

        RealNumericAnswerModel GetRealNumericAnswerModel(Identity identity);

        MaskedTextAnswerModel GetTextAnswerModel(Identity identity);

        SingleOptionAnswerModel GetSingleOptionAnswerModel(Identity identity);
        
        bool IsValid(Identity identity);

        bool IsEnabled(Identity entityIdentity);
    }
}