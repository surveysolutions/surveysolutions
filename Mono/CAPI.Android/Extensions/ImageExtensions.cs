using Android.App;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Views;
using Android.Widget;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;

namespace CAPI.Android.Extensions
{
    public static class ImageExtensions
    {
        public static void AttachImage(this TextView view, AnswerViewModel answer)
        {
            if (string.IsNullOrEmpty(answer.ImagePublicKey))
            {
                return;
            }
            var file = CapiApplication.FileStorageService.RetrieveFile(answer.ImagePublicKey);
            if (file == null)
                return;

            var parentActivity = view.Context as Activity;
            if(parentActivity==null)
                return;

            Display display = parentActivity.WindowManager.DefaultDisplay;
            using (var img = file.Content)
            {
                using (Bitmap bm =
                    BitmapFactory.DecodeStream(img))
                {
                    using (var resized = Resize(bm, display))
                    {
                        view.SetCompoundDrawablesWithIntrinsicBounds(new BitmapDrawable(resized), null, null, null);
                    }
                    view.Text = string.Empty;
                    view.SetBackgroundResource(Resource.Drawable.questionShape);
                    view.SetPadding(10, 10, 0, 10);
                }
            }

        }

        private const int ScreenCoeff = 5;
        static Bitmap Resize(Bitmap bm, Display display)
        {
            int approptiateHeight = display.Height / ScreenCoeff;
            double coff = (double)approptiateHeight / bm.Height;
            int newWidth = (int)(coff * bm.Width);
            return Bitmap.CreateScaledBitmap(bm, newWidth, approptiateHeight, true);
        }
    }
}