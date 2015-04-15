using System;
using System.Collections.Generic;

using WB.Core.SharedKernels.DataCollection.Aggregates;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Entities
{
    public class InterviewModel : IInterview
    {
        public InterviewModel()
        {
            Answers = new Dictionary<string, AbstractInterviewQuestionModel>();
            this.GroupsAndRosters = new Dictionary<string, InterviewGroupModel>();
            QuestionIdToQuestionModelTypeMap = new Dictionary<Guid, QuestionModelType>();
            IsInProgress = true;
        }
        public Guid QuestionnaireId { get; set; }
        public long QuestionnaireVersion { get; set; }

        public Guid Id { get; set; }

        public Dictionary<string, AbstractInterviewQuestionModel> Answers { get; set; }
        public Dictionary<string, InterviewGroupModel> GroupsAndRosters { get; set; }

        public Dictionary<Guid, QuestionModelType> QuestionIdToQuestionModelTypeMap { get; set; }

        public bool HasErrors { get; set; }
        public bool IsInProgress { get; set; }

        public Dictionary<QuestionModelType, Func<AbstractInterviewQuestionModel>> QuestionModelTypeToModelActivatorMap = new Dictionary<QuestionModelType, Func<AbstractInterviewQuestionModel>>
                {
                    { QuestionModelType.SingleOption, () => new SingleOptionQuestionModel()},
                    { QuestionModelType.LinkedSingleOption, () => new LinkedSingleOptionQuestionModel()},
                    { QuestionModelType.MultiOption, () => new MultiOptionQuestionModel()},
                    { QuestionModelType.LinkedMultiOption, () => new LinkedMultiOptionQuestionModel()},
                    { QuestionModelType.IntegerNumeric, () => new IntegerNumericQuestionModel()},
                    { QuestionModelType.RealNumeric, () => new RealNumericQuestionModel()},
                    { QuestionModelType.MaskedText, () => new MaskedTextQuestionModel()},
                    { QuestionModelType.TextList, () => new TextListQuestionModel()},
                    { QuestionModelType.QrBarcode, () => new QrBarcodeQuestionModel()},
                    { QuestionModelType.Multimedia, () => new MultimediaQuestionModel()},
                    { QuestionModelType.DateTime, () => new DateTimeQuestionModel()},
                    { QuestionModelType.GpsCoordinates, () => new GpsCoordinatesQuestionModel()}
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