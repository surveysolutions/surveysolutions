using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidApp.Core.Model.ViewModel.QuestionnaireDetails;

namespace AndroidApp.Extensions
{
    public static class ImageExtensions
    {
        public static void AttachImage(this TextView view, AnswerViewModel answer)
        {
            if (!string.IsNullOrEmpty(answer.ImagePublicKey))
            {
                using (var img = CapiApplication.FileStorageService.RetrieveFile(answer.ImagePublicKey).Content)
                {
                    using (Bitmap bm =
                        BitmapFactory.DecodeStream(img))
                    {
                        using (var resized = Resize(bm))
                        {
                            view.SetCompoundDrawablesWithIntrinsicBounds(null, new BitmapDrawable(resized), null, null);
                        }
                        view.Text = string.Empty;
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