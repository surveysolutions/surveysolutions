using System.Windows;
using System.Windows.Controls;
using RavenQuestionnaire.Core.Views.Answer;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace QApp.Helpers.DataTemplateSelectors
{
    public class SingleOptionDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate SingleOptionWithImage { get; set; }

        public DataTemplate SingleOptionWithoutImage { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is CompleteAnswerView)
            {
                var answer = item as CompleteAnswerView;
                if (answer.AnswerType == AnswerType.Image)
                    return SingleOptionWithImage;
                return SingleOptionWithoutImage;
            }
            return null;
        }
    }
}
