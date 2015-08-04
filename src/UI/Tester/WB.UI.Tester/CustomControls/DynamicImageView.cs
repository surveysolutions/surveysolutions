using System;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Util;
using Android.Widget;
using Cirrious.CrossCore.Core;
using Cirrious.MvvmCross.Binding.Droid.ResourceHelpers;
using Cirrious.MvvmCross.Plugins.DownloadCache;

namespace WB.UI.Tester.CustomControls
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
                    HttpImageUrl = typedArray.GetString(attributeId);
                }
            }
            typedArray.Recycle();
        }

        public string HttpImageUrl
        {
            get { return Image.ImageUrl; }
            set { Image.ImageUrl = value; }
        }

        public DynamicImageView(Context context)
            : base(context)
        {
        }

        protected DynamicImageView(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        public MvxDynamicImageHelper<Bitmap> Image { get { return this.imageHelper; } }

        private void ImageHelperOnImageChanged(object sender, MvxValueEventArgs<Bitmap> mvxValueEventArgs)
        {
            SetImageBitmap(mvxValueEventArgs.Value);
            this.Invalidate();
        }
    }
}