using System;
using System.IO;
using MvvmCross.Binding;
using MvvmCross.WeakSubscription;
using Xamarin.Controls;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class SignatureBinding : BaseBinding<SignaturePadView, byte[]>
    {
        private IDisposable strokeCompletedSubscription;
        private IDisposable clearedSubscription;
        
        public override MvxBindingMode DefaultMode => MvxBindingMode.TwoWay;

        public SignatureBinding(SignaturePadView androidControl) : base(androidControl)
        {
        }

        public override void SubscribeToEvents()
        {
            base.SubscribeToEvents();

            if(this.Target == null)
                return;

            strokeCompletedSubscription = this.Target.SignaturePadCanvas.WeakSubscribe(
                nameof(this.Target.SignaturePadCanvas.StrokeCompleted),
                SignaturePadCanvasOnStrokeCompleted);

            clearedSubscription = Target.SignaturePadCanvas.WeakSubscribe(
                nameof(this.Target.SignaturePadCanvas.Cleared),
                SignaturePadCanvasOnStrokeCompleted);
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
            if (isDisposing)
            {
                clearedSubscription?.Dispose();
                clearedSubscription = null;
                
                strokeCompletedSubscription?.Dispose();
                strokeCompletedSubscription = null;
            }

            base.Dispose(isDisposing);
        }
    }
}
