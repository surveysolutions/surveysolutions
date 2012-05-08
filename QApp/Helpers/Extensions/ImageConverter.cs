using System;
using System.IO;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Media.Imaging;
using RavenQuestionnaire.Core.Services;

namespace QApp.Helpers.Extensions
{
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
}
