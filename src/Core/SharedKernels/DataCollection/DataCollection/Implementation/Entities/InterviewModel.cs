using System;
using System.Collections.Generic;

using WB.Core.SharedKernels.DataCollection.Aggregates;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Entities
{
    public class InterviewModel : IInterview
    {
        public InterviewModel()
        {
            Answers = new Dictionary<string, AbstractInterviewAnswerModel>();
            this.Groups = new Dictionary<string, InterviewGroupModel>();
            QuestionIdToQuestionModelTypeMap = new Dictionary<Guid, QuestionModelType>();
            IsInProgress = true;
        }
        public Guid QuestionnaireId { get; set; }
        public long QuestionnaireVersion { get; set; }

        public Guid Id { get; set; }

        public Dictionary<string, AbstractInterviewAnswerModel> Answers { get; set; }
        public Dictionary<string, InterviewGroupModel> Groups { get; set; }
        public Dictionary<string, List<string>> RosterInstancesIds { get; set; }

        public Dictionary<Guid, QuestionModelType> QuestionIdToQuestionModelTypeMap { get; set; }

        public bool HasErrors { get; set; }
        public bool IsInProgress { get; set; }

        public Dictionary<QuestionModelType, Func<AbstractInterviewAnswerModel>> QuestionModelTypeToModelActivatorMap = new Dictionary<QuestionModelType, Func<AbstractInterviewAnswerModel>>
                {
                    { QuestionModelType.SingleOption, () => new SingleOptionAnswerModel()},
                    { QuestionModelType.LinkedSingleOption, () => new LinkedSingleOptionAnswerModel()},
                    { QuestionModelType.MultiOption, () => new MultiOptionAnswerModel()},
                    { QuestionModelType.LinkedMultiOption, () => new LinkedMultiOptionAnswerModel()},
                    { QuestionModelType.IntegerNumeric, () => new IntegerNumericAnswerModel()},
                    { QuestionModelType.RealNumeric, () => new RealNumericAnswerModel()},
                    { QuestionModelType.MaskedText, () => new MaskedTextAnswerModel()},
                    { QuestionModelType.TextList, () => new TextListAnswerModel()},
                    { QuestionModelType.QrBarcode, () => new QrBarcodeAnswerModel()},
                    { QuestionModelType.Multimedia, () => new MultimediaAnswerModel()},
                    { QuestionModelType.DateTime, () => new DateTimeAnswerModel()},
                    { QuestionModelType.GpsCoordinates, () => new GpsCoordinatesAnswerModel()}
                };

        public GpsCoordinatesAnswerModel GetGpsCoordinatesAnswerModel(Identity identity)
        {
            return GetQuestionModel<GpsCoordinatesAnswerModel>(identity);
        }

        public DateTimeAnswerModel GetDateTimeAnswerModel(Identity identity)
        {
            return GetQuestionModel<DateTimeAnswerModel>(identity);
        }

        public MultimediaAnswerModel GetMultimediaAnswerModel(Identity identity)
        {
            return GetQuestionModel<MultimediaAnswerModel>(identity);
        }

        public QrBarcodeAnswerModel GetQrBarcodeAnswerModel(Identity identity)
        {
            return GetQuestionModel<QrBarcodeAnswerModel>(identity);
        }

        public TextListAnswerModel GetTextListAnswerModel(Identity identity)
        {
            return GetQuestionModel<TextListAnswerModel>(identity);
        }

        public LinkedSingleOptionAnswerModel GetLinkedSingleOptionAnswerModel(Identity identity)
        {
            return GetQuestionModel<LinkedSingleOptionAnswerModel>(identity);
        }

        public MultiOptionAnswerModel GetMultiOptionAnswerModel(Identity identity)
        {
            return GetQuestionModel<MultiOptionAnswerModel>(identity);
        }

        public LinkedMultiOptionAnswerModel GetLinkedMultiOptionAnswerModel(Identity identity)
        {
            return GetQuestionModel<LinkedMultiOptionAnswerModel>(identity);
        }

        public IntegerNumericAnswerModel GetIntegerNumericAnswerModel(Identity identity)
        {
            return GetQuestionModel<IntegerNumericAnswerModel>(identity);
        }

        public RealNumericAnswerModel GetRealNumericAnswerModel(Identity identity)
        {
            return GetQuestionModel<RealNumericAnswerModel>(identity);
        }

        public MaskedTextAnswerModel GetTextAnswerModel(Identity identity)
        {
            return GetQuestionModel<MaskedTextAnswerModel>(identity);
        }

        public SingleOptionAnswerModel GetSingleAnswerModel(Identity identity)
        {
            return GetQuestionModel<SingleOptionAnswerModel>(identity);
        }

        private T GetQuestionModel<T>(Identity identity) where T : AbstractInterviewAnswerModel
        {
            var questionId = ConversionHelper.ConvertIdentityToString(identity);
            if (!Answers.ContainsKey(questionId)) return null;
            return (T)Answers[questionId];
        }

       
    }
}