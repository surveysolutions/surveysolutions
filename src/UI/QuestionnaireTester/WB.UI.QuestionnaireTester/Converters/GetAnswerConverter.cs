using System;
using System.Globalization;
using System.Linq;
using Cirrious.CrossCore.Converters;
using Microsoft.Practices.ServiceLocation;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities.QuestionModels;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.UI.QuestionnaireTester.Converters
{
    public class GetAnswerConverter : MvxValueConverter<AbstractInterviewAnswerModel, string>
    {
        private static IPlainRepository<InterviewModel> interviewRepository
        {
            get { return ServiceLocator.Current.GetInstance<IPlainRepository<InterviewModel>>(); }
        }

        private static IPlainRepository<QuestionnaireModel> questionnaireRepository
        {
            get { return ServiceLocator.Current.GetInstance<IPlainRepository<QuestionnaireModel>>(); }
        }

        protected override string Convert(AbstractInterviewAnswerModel value, Type targetType, object parameter, CultureInfo culture)
        {
            string interviewId = (string) parameter;

            string answerAsString = string.Empty;
            if (value != null)
            {
                TypeSwitch.Do(value, 
                    TypeSwitch.Case<MaskedTextAnswerModel>((maskedTextAnswerModel)=>answerAsString = maskedTextAnswerModel.Answer),
                    TypeSwitch.Case<IntegerNumericAnswerModel>((integerAnswerModel)=>answerAsString = integerAnswerModel.Answer.ToString(culture)),
                    TypeSwitch.Case<RealNumericAnswerModel>((realAnswerModel)=>answerAsString = realAnswerModel.Answer.ToString(culture)),
                    TypeSwitch.Case<DateTimeAnswerModel>((dateAnswerModel)=>answerAsString = dateAnswerModel.Answer.ToString("d", culture)),
                    TypeSwitch.Case<SingleOptionAnswerModel>((singleOptionAnswerModel) => answerAsString = GetAnswerOnSingleOptionQuestionAsString(interviewId, singleOptionAnswerModel)),
                    TypeSwitch.Case<MultiOptionAnswerModel>((multiOptionAnswerModel) => answerAsString = GetAnswerOnMultiOptionQuestionAsString(interviewId, multiOptionAnswerModel)));
            }

            return answerAsString;
        }

        private static QuestionnaireModel GetQuestionnaire(string interviewId)
        {
            var interview = interviewRepository.Get(interviewId);
            return questionnaireRepository.Get(interview.QuestionnaireId);
        }

        private static string GetAnswerOnSingleOptionQuestionAsString(string interviewId, SingleOptionAnswerModel singleOptionAnswerModel)
        {
            var singleOptionQuestionModel = GetQuestionnaire(interviewId).Questions[singleOptionAnswerModel.Id] as SingleOptionQuestionModel;
            return singleOptionQuestionModel != null ? singleOptionQuestionModel.Options.FirstOrDefault(_ => _.Id == singleOptionAnswerModel.Answer).Title : string.Empty;
        }

        private static string GetAnswerOnMultiOptionQuestionAsString(string interviewId, MultiOptionAnswerModel multiOptionAnswerModel)
        {
            var multiOptionQuestionModel = GetQuestionnaire(interviewId).Questions[multiOptionAnswerModel.Id] as MultiOptionQuestionModel;
            return multiOptionQuestionModel != null
                ? string.Join(",",
                    multiOptionQuestionModel.Options.Where(
                        _ => multiOptionAnswerModel.Answers.Contains(_.Id)).Select(_ => _.Title))
                : string.Empty;
        }
    }
}