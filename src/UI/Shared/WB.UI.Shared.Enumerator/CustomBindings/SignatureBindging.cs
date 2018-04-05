using System;
using System.IO;
using MvvmCross.Binding;
using Xamarin.Controls;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class SignatureBinding : BaseBinding<SignaturePadView, byte[]>
    {
        public override MvxBindingMode DefaultMode => MvxBindingMode.TwoWay;

        public SignatureBinding(SignaturePadView androidControl) : base(androidControl)
        {
        }

        public override void SubscribeToEvents()
        {
            base.SubscribeToEvents();

            Target.SignaturePadCanvas.StrokeCompleted += SignaturePadCanvasOnStrokeCompleted;
            Target.SignaturePadCanvas.Cleared += SignaturePadCanvasOnStrokeCompleted;
        }

        private async void SignaturePadCanvasOnStrokeCompleted(object sender, EventArgs eventArgs)
        {
            if (!this.Target.IsBlank)
            {
                using (var imageStream = await Target.SignaturePadCanvas.GetImageStreamAsync(SignatureImageFormat.Png))
                {
                    imageStream.Seek(0, SeekOrigin.Begin);

                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        await imageStream.CopyToAsync(memoryStream);
                        await imageStream.FlushAsync();
                        memoryStream.Seek(0, SeekOrigin.Begin);
                        FireValueChanged(memoryStream.ToArray());
                    }
                }
            }
            else
            {
                FireValueChanged(null);
            }
        }

        protected override void SetValueToView(SignaturePadView control, byte[] value)
        {
            if (value == null || value.Length == 0)
            {
                this.Target.Clear();
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            if (IsDisposed)
                return;

            if (Target?.SignaturePadCanvas != null && Target?.SignaturePadCanvas.Handle != IntPtr.Zero)
            {
                Target.SignaturePadCanvas.StrokeCompleted -= SignaturePadCanvasOnStrokeCompleted;
                Target.SignaturePadCanvas.Cleared -= SignaturePadCanvasOnStrokeCompleted;
            }

            base.Dispose(isDisposing);
        }
    }
}
