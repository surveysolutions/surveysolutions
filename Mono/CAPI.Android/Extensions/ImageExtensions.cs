using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Widget;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;

namespace CAPI.Android.Extensions
{
    public static class ImageExtensions
    {
        public static void AttachImage(this TextView view, AnswerViewModel answer)
        {
            if (!string.IsNullOrEmpty(answer.ImagePublicKey))
            {
                var file = CapiApplication.FileStorageService.RetrieveFile(answer.ImagePublicKey);
                if(file==null)
                    return;
                using (var img = file.Content)
                {
                    using (Bitmap bm =
                        BitmapFactory.DecodeStream(img))
                    {
                        using (var resized = Resize(bm))
                        {
                            view.SetCompoundDrawablesWithIntrinsicBounds(null, null, null, new BitmapDrawable(resized));
                        }
                        view.Text = string.Empty;
                        view.SetBackgroundResource(Resource.Drawable.questionShape);
                    }
                }
            }
        }
        private const int IMAGE_HEIGHT = 400;
        static Bitmap Resize(Bitmap bm)
        {
            var coff = IMAGE_HEIGHT / bm.Height;
            var newWidth = coff * bm.Width;
            return Bitmap.CreateScaledBitmap(bm, newWidth, IMAGE_HEIGHT, true);
        }
    }
}