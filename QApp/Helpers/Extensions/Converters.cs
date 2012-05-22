using System;
using System.IO;
using System.Linq;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Media.Imaging;
using RavenQuestionnaire.Core.Services;
using RavenQuestionnaire.Core.Views.Answer;
using RavenQuestionnaire.Core.Views.Question;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;

namespace QApp.Helpers.Extensions
{
    public class ChangeObjectMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            ///bad !!! Need refactoring
            try
            {
                if (values.Count() == 0)
                    return null;
                CompleteQuestionView param = null;
                if (values[0] is CompleteQuestionView)
                    param = values[0] as CompleteQuestionView;
                else
                    param = ((ViewModel.Question)(values[0])).QuestionData.Question;
                if (param != null)
                {
                    var answers = new CompleteAnswerView[1]
                                  {
                                      new CompleteAnswerView(param.PublicKey,
                                                             new CompleteAnswer
                                                                 {
                                                                     AnswerText = values[1].ToString(),
                                                                     AnswerValue = values[1].ToString(),
                                                                     AnswerType = AnswerType.Select,
                                                                     PublicKey = param.Answers[0].PublicKey,
                                                                     Selected = true
                                                                 })
                                  };
                    param.Answers = answers;
                }
                return param;
            }
            catch (Exception)
            {
               return null;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                IFileStorageService fileStorage = Initializer.Kernel.GetService(typeof(RavenFileStorageService)) as RavenFileStorageService;
                if (fileStorage != null)
                {
                    var ms = new MemoryStream(fileStorage.RetrieveFile(((RavenQuestionnaire.Core.Views.Card.CardView)(value)).Thumb));
                    var image = new BitmapImage();
                    image.BeginInit();
                    image.StreamSource = ms;
                    image.DecodePixelWidth = 200;
                    image.DecodePixelHeight = 200;
                    image.EndInit();
                    return image;
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class EnabledDisabledConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                var question = value as CompleteQuestionView;
                if (question.Enabled)
                    return true;
                return false;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class FromBoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
                return ((bool)value) ? "1" : "0.25";
            return "1";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
