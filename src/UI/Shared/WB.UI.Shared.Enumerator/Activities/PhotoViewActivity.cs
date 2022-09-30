﻿using Android.App;
using Android.Content.PM;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Views;
using Android.Widget;
using Autofac.Core;
using ImageViews.Photo;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.UI.Shared.Enumerator.Activities
{
    [Activity(WindowSoftInputMode = SoftInput.StateHidden, 
        Theme = "@style/AppTheme", 
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize,
        HardwareAccelerated = false,
        Exported = false)]
    public class PhotoViewActivity : BaseActivity<PhotoViewViewModel>
    {
        protected override int ViewResourceId => Resource.Layout.interview_photo_view;
        
        public override void OnBackPressed()
        {
            this.Cancel();
        }

        private void Cancel()
        {
            this.Finish();
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
        }

        private bool imageCleared;

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            if (!imageCleared)
            {
                imageCleared = true;

                PhotoView i = FindViewById<PhotoView>(Resource.Id.image);
                if (i != null)
                {
                    if (i.Drawable is BitmapDrawable d)
                    {
                        Bitmap bitmap = d.Bitmap;
                        if (bitmap != null)
                        {
                            bitmap.Recycle();
                            bitmap.Dispose();
                        }
                    }

                    i.SetImageBitmap(null);
                    i.SetImageDrawable(null);
                }
            }
        }
    }
}
