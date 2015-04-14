using System;
using System.Collections.Generic;

using WB.Core.SharedKernels.DataCollection.Aggregates;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Entities
{
    public enum QuestionModelType
    {
        SingleOptionQuestionModel,
        LinkedSingleOptionQuestionModel,
        MultiOptionQuestionModel,
        LinkedMultiOptionQuestionModel,
        IntegerNumericQuestionModel,
        RealNumericQuestionModel,
        MaskedTextQuestionModel,
        TextListQuestionModel,
        QrBarcodeQuestionModel,
        MultimediaQuestionModel,
        DateTimeQuestionModel,
        GpsCoordinatesQuestionModel,
    }

    public class InterviewModel : IInterview
    {
        public InterviewModel()
        {
            Answers = new Dictionary<string, AbstractInterviewQuestionModel>();
            Rosters = new Dictionary<string, InterviewRosterModel>();
            Groups = new Dictionary<string, InterviewGroupModel>();
            QuestionIdToQuestionModelTypeMap = new Dictionary<Guid, QuestionModelType>();
        }
        public Guid QuestionnaireId { get; set; }
        public long QuestionnaireVersion { get; set; }

        public Guid Id { get; set; }

        public Dictionary<string, AbstractInterviewQuestionModel> Answers { get; set; }
        public Dictionary<string, InterviewRosterModel> Rosters { get; set; }
        public Dictionary<string, InterviewGroupModel> Groups { get; set; }

        public Dictionary<Guid, QuestionModelType> QuestionIdToQuestionModelTypeMap { get; set; }

        public Dictionary<QuestionModelType, Func<AbstractInterviewQuestionModel>> QuestionModelTypeToModelActivatorMap = new Dictionary<QuestionModelType, Func<AbstractInterviewQuestionModel>>
                {
                    { QuestionModelType.SingleOptionQuestionModel, () => new SingleOptionQuestionModel()},
                    { QuestionModelType.LinkedSingleOptionQuestionModel, () => new LinkedSingleOptionQuestionModel()},
                    { QuestionModelType.MultiOptionQuestionModel, () => new MultiOptionQuestionModel()},
                    { QuestionModelType.LinkedMultiOptionQuestionModel, () => new LinkedMultiOptionQuestionModel()},
                    { QuestionModelType.IntegerNumericQuestionModel, () => new IntegerNumericQuestionModel()},
                    { QuestionModelType.RealNumericQuestionModel, () => new RealNumericQuestionModel()},
                    { QuestionModelType.MaskedTextQuestionModel, () => new MaskedTextQuestionModel()},
                    { QuestionModelType.TextListQuestionModel, () => new TextListQuestionModel()},
                    { QuestionModelType.QrBarcodeQuestionModel, () => new QrBarcodeQuestionModel()},
                    { QuestionModelType.MultimediaQuestionModel, () => new MultimediaQuestionModel()},
                    { QuestionModelType.DateTimeQuestionModel, () => new DateTimeQuestionModel()},
                    { QuestionModelType.GpsCoordinatesQuestionModel, () => new GpsCoordinatesQuestionModel()}
                };

        public MaskedTextQuestionModel GetTextQuestionModel(Identity identity)
        {
            return GetQuestionModel<MaskedTextQuestionModel>(identity);
        }

        private T GetQuestionModel<T>(Identity identity) where T : AbstractInterviewQuestionModel
        {
            var questionId = ConversionHelper.ConvertIdentityToString(identity);
            if (!Answers.ContainsKey(questionId)) return null;
            return (T)Answers[questionId];
        }
    }
}