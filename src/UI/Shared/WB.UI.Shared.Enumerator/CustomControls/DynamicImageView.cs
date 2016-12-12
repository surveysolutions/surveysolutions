using System;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Util;
using Android.Widget;
using MvvmCross.Platform.Core;
using MvvmCross.Binding.Droid.ResourceHelpers;
using MvvmCross.Platform;
using MvvmCross.Plugins.DownloadCache;

namespace WB.UI.Shared.Enumerator.CustomControls
{
    public class DynamicImageView : ImageView
    {
        private readonly MvxDynamicImageHelper<Bitmap> imageHelper;

        public DynamicImageView(Context context, IAttributeSet attrs)
            : base(context, attrs)
        { 
            this.imageHelper = new MvxDynamicImageHelper<Bitmap>();
            this.imageHelper.ImageChanged += ImageHelperOnImageChanged;
            var typedArray = context.ObtainStyledAttributes(attrs, MvxAndroidBindingResource.Instance.ImageViewStylableGroupId);

            int numStyles = typedArray.IndexCount;
            for (var i = 0; i < numStyles; ++i)
            {
                int attributeId = typedArray.GetIndex(i);
                if (attributeId == MvxAndroidBindingResource.Instance.SourceBindId)
                {
                    this.HttpImageUrl = typedArray.GetString(attributeId);
                }
            }
            typedArray.Recycle();
        }

        public string HttpImageUrl
        {
            get { return this.Image.ImageUrl; }
            set { this.Image.ImageUrl = value; }
        }

        public DynamicImageView(Context context)
            : base(context)
        {
        }

        protected DynamicImageView(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        public MvxDynamicImageHelper<Bitmap> Image => this.imageHelper;

        private void ImageHelperOnImageChanged(object sender, MvxValueEventArgs<Bitmap> mvxValueEventArgs)
        {
            var mvxMainThreadDispatcher = Mvx.Resolve<IMvxMainThreadDispatcher>();
            mvxMainThreadDispatcher.RequestMainThreadAction(() =>
            {
                this.SetImageBitmap(mvxValueEventArgs.Value);
                this.Invalidate();
            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                this.imageHelper.Dispose();
            }
        }
    }
}