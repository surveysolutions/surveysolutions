using System;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Util;
using Android.Widget;
using MvvmCross;
using MvvmCross.Base;

namespace WB.UI.Shared.Enumerator.CustomControls
{
    //public class HttpImageView : ImageView
    //{
    //    private readonly MvxDynamicImageHelper<Bitmap> _imageHelper;

    //    public HttpImageView(Context context, IAttributeSet attrs)
    //        : base(context, attrs)
    //    { 
    //        this._imageHelper = new MvxDynamicImageHelper<Bitmap>();

    //        this._imageHelper.ImageChanged += this.ImageHelperOnImageChanged;
    //        var typedArray = context.ObtainStyledAttributes(attrs, MvxAndroidBindingResource.Instance.ImageViewStylableGroupId);

    //        int numStyles = typedArray.IndexCount;
    //        for (var i = 0; i < numStyles; ++i)
    //        {
    //            int attributeId = typedArray.GetIndex(i);
    //            if (attributeId == MvxAndroidBindingResource.Instance.SourceBindId)
    //            {
    //                this.HttpImageUrl = typedArray.GetString(attributeId);
    //            }
    //        }
    //        typedArray.Recycle();
    //    }

    //    public string HttpImageUrl
    //    {
    //        get { return this.Image.ImageUrl; }
    //        set { this.Image.ImageUrl = value; }
    //    }

    //    public HttpImageView(Context context)
    //        : base(context)
    //    {
    //    }

    //    protected HttpImageView(IntPtr javaReference, JniHandleOwnership transfer)
    //        : base(javaReference, transfer)
    //    {
    //    }

    //    public MvxDynamicImageHelper<Bitmap> Image { get { return this._imageHelper; } }

    //    private void ImageHelperOnImageChanged(object sender, MvxValueEventArgs<Bitmap> mvxValueEventArgs)
    //    {
    //        var mvxMainThreadDispatcher = Mvx.Resolve<IMvxMainThreadDispatcher>();
    //        mvxMainThreadDispatcher.RequestMainThreadAction(() =>
    //        {
    //            this.SetImageBitmap(mvxValueEventArgs.Value);
    //            this.Invalidate();
    //        });
    //    }

    //    protected override void Dispose(bool disposing)
    //    {
    //        base.Dispose(disposing);
    //        if (disposing)
    //        {
    //            this._imageHelper.Dispose();
    //        }
    //    }
    //}
}
