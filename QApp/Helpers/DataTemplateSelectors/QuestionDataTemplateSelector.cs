using System.Windows;
using System.Windows.Controls;
using RavenQuestionnaire.Core.Views.Question;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace QApp.Helpers.DataTemplateSelectors
{
    public class QuestionDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DropDownTemplate { get; set; }

        public DataTemplate DateTimeTemplate { get; set; }

        public DataTemplate GpsCoordinatesTemplate { get; set; }

        public DataTemplate MultyOptionTemplate { get; set; }

        public DataTemplate NumericTemplate { get; set; }

        public DataTemplate TextTemplate { get; set; }

        public DataTemplate PercentageTemplate { get; set; }

        public DataTemplate DefaultTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is CompleteQuestionView)
            {
                var question = item as CompleteQuestionView;
                switch (question.QuestionType)
                {
                    //case QuestionType.ExtendedDropDownList:
                    //    return DropDownTemplate;
                    case QuestionType.DropDownList:
                        return DropDownTemplate;
                    case QuestionType.DateTime:
                        return DateTimeTemplate;
                    case QuestionType.GpsCoordinates:
                        return GpsCoordinatesTemplate;
                    case QuestionType.MultyOption:
                        return MultyOptionTemplate;
                    case QuestionType.Numeric:
                        return NumericTemplate;
                    case QuestionType.Text:
                        return TextTemplate;
                    case QuestionType.Percentage:
                        return PercentageTemplate;
                    default:
                        return DefaultTemplate;
                }
            }
            return null;
        }
    }
}
