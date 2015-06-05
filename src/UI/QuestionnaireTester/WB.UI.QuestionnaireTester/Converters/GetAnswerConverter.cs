using System;
using System.Globalization;
using System.Linq;
using Cirrious.CrossCore.Converters;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities.QuestionModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.UI.QuestionnaireTester.Converters
{
    public class GetAnswerConverter : MvxValueConverter<BaseInterviewAnswer, string>
    {
        private static IStatefulInterviewRepository interviewRepository
        {
            get { return ServiceLocator.Current.GetInstance<IStatefulInterviewRepository>(); }
        }

        private static IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository
        {
            get { return ServiceLocator.Current.GetInstance<IPlainKeyValueStorage<QuestionnaireModel>>(); }
        }

        protected override string Convert(BaseInterviewAnswer value, Type targetType, object parameter, CultureInfo culture)
        {
            string interviewId = (string) parameter;

            string answerAsString = string.Empty;
            if (value != null)
            {
                TypeSwitch.Do(value, 
                    TypeSwitch.Case<MaskedTextAnswer>((maskedTextAnswerModel)=>answerAsString = maskedTextAnswerModel.Answer),
                    TypeSwitch.Case<IntegerNumericAnswer>((integerAnswerModel) => answerAsString = GetAnswerOnIntegerQuestionAsString(integerAnswerModel, culture)),
                    TypeSwitch.Case<RealNumericAnswer>((realAnswerModel)=>answerAsString = GetAnswerOnRealQuestionAsString(realAnswerModel,culture)),
                    TypeSwitch.Case<DateTimeAnswer>((dateAnswerModel)=>answerAsString = dateAnswerModel.Answer.ToString("d", culture)),
                    TypeSwitch.Case<SingleOptionAnswer>((singleOptionAnswerModel) => answerAsString = GetAnswerOnSingleOptionQuestionAsString(interviewId, singleOptionAnswerModel)),
                    TypeSwitch.Case<MultiOptionAnswer>((multiOptionAnswerModel) => answerAsString = GetAnswerOnMultiOptionQuestionAsString(interviewId, multiOptionAnswerModel)));
            }

            return answerAsString;
        }

        private static QuestionnaireModel GetQuestionnaire(string interviewId)
        {
            var interview = interviewRepository.Get(interviewId);
            return questionnaireRepository.GetById(interview.QuestionnaireId);
        }

        private static string GetAnswerOnIntegerQuestionAsString(IntegerNumericAnswer integerAnswer, CultureInfo culture)
        {
            return integerAnswer!=null && integerAnswer.Answer.HasValue ? integerAnswer.Answer.Value.ToString(culture) : string.Empty;
        }

        private static string GetAnswerOnRealQuestionAsString(RealNumericAnswer realAnswer, CultureInfo culture)
        {
            return realAnswer != null && realAnswer.Answer.HasValue ? realAnswer.Answer.Value.ToString(culture) : string.Empty;
        }

        private static string GetAnswerOnSingleOptionQuestionAsString(string interviewId, SingleOptionAnswer singleOptionAnswer)
        {
            var singleOptionQuestionModel = GetQuestionnaire(interviewId).Questions[singleOptionAnswer.Id] as SingleOptionQuestionModel;
            return singleOptionQuestionModel != null ? singleOptionQuestionModel.Options.FirstOrDefault(_ => _.Value == singleOptionAnswer.Answer).Title : string.Empty;
        }

        private static string GetAnswerOnMultiOptionQuestionAsString(string interviewId, MultiOptionAnswer multiOptionAnswer)
        {
            var multiOptionQuestionModel = GetQuestionnaire(interviewId).Questions[multiOptionAnswer.Id] as MultiOptionQuestionModel;
            return multiOptionQuestionModel != null
                ? string.Join(",",
                    multiOptionQuestionModel.Options.Where(
                        _ => multiOptionAnswer.Answers.Contains(_.Value)).Select(_ => _.Title))
                : string.Empty;
        }
    }
}