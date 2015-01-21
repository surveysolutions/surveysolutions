using System;
using System.Collections.Generic;
using System.IO;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Provider;
using Android.Views;
using Android.Widget;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Cirrious.MvvmCross.Plugins.PictureChooser;
using WB.Core.BoundedContexts.Capi;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.UI.Shared.Android.Controls.ScreenItems
{
    public class PictureQuestionView : AbstractQuestionView
    {
        public PictureQuestionView(Context context, IMvxAndroidBindingContext bindingActivity, QuestionViewModel source,
                                Guid questionnairePublicKey, ICommandService commandService,
                                IAnswerOnQuestionCommandService answerCommandService, IAuthentication membership, IPlainInterviewFileStorage plainInterviewFileStorage)
            : base(context, bindingActivity, source, questionnairePublicKey, commandService, answerCommandService, membership)
        {
            this.plainInterviewFileStorage = plainInterviewFileStorage;

            if (this.IsThereAnAppToTakePictures())
            {
                this.pictureChooserTask = Mvx.Resolve<IMvxPictureChooserTask>();
                var wrapper = new LinearLayout(this.CurrentContext)
                {
                    LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.FillParent, ViewGroup.LayoutParams.FillParent)
                };
                wrapper.Orientation = Orientation.Horizontal;

                this.llWrapper.AddView(wrapper);

                var imageLayoutParams = new LayoutParams(ViewGroup.LayoutParams.FillParent, ViewGroup.LayoutParams.WrapContent, 9);
                
                this.ivImage = new ImageView(this.CurrentContext) { LayoutParameters = imageLayoutParams };
                ivImage.SetAdjustViewBounds(true);
                wrapper.AddView(this.ivImage);

                var buttonLayoutParams = new LayoutParams(ViewGroup.LayoutParams.WrapContent,
                    ViewGroup.LayoutParams.WrapContent, 1);

                var button = new Button(this.CurrentContext) { LayoutParameters = buttonLayoutParams };

                SetIcon(button, this.IsPicturePresent() ? Remove : TakePicture);
                button.Click += this.BtnTakePictureClick;
                
                wrapper.AddView(button);
                
                this.PutAnswerStoredInModelToUI();
            }
            else
            {
                var tvWarning = new TextView(this.CurrentContext);
                tvWarning.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.WrapContent,
                    ViewGroup.LayoutParams.WrapContent);
                tvWarning.Text = "Camera is absent";
                this.llWrapper.AddView(tvWarning);
            }
        }

        private readonly IPlainInterviewFileStorage plainInterviewFileStorage;
        private readonly IMvxPictureChooserTask pictureChooserTask;
        private const int TakePicture = global::Android.Resource.Drawable.IcMenuCamera;
        private const int Remove = global::Android.Resource.Drawable.IcMenuDelete;

        protected readonly ImageView ivImage;

        protected ValueQuestionViewModel TypedMode
        {
            get { return this.Model as ValueQuestionViewModel; }
        }

        protected bool IsThereAnAppToTakePictures()
        {
            Intent intent = new Intent(MediaStore.ActionImageCapture);
            IList<ResolveInfo> availableActivities = this.CurrentContext.PackageManager.QueryIntentActivities(intent, PackageInfoFlags.MatchDefaultOnly);
            return availableActivities != null && availableActivities.Count > 0;
        }

        protected void BtnTakePictureClick(object sender, EventArgs e)
        {
            var button = sender as Button;
            if (button == null)
                return;

            if (IsPicturePresent())
            {
                this.plainInterviewFileStorage.RemoveInterviewBinaryData(this.QuestionnairePublicKey, Model.AnswerString);
                ivImage.SetImageDrawable(null);
                this.SavePictureToAR(string.Empty);
                SetIcon(button, TakePicture);
            }
            else
            {
                pictureChooserTask.TakePicture(400, 95, (s) => OnPicture(s, button), () => { });
            }
        }

        private void OnPicture(Stream pictureStream, Button button)
        {
            var pictureFileName = String.Format("{0}{1}.jpg", Model.Variable,
                string.Join("-", Model.PublicKey.InterviewItemPropagationVector));
            byte[] data = null;
            using (var memoryStream = new MemoryStream())
            {
                pictureStream.CopyTo(memoryStream);
                data = memoryStream.ToArray();
                this.plainInterviewFileStorage.StoreInterviewBinaryData(this.QuestionnairePublicKey, pictureFileName, data);
            }

            Bitmap bitmap = BitmapFactory.DecodeByteArray(data, 0, data.Length);
            ivImage.SetImageBitmap(bitmap);
            this.SavePictureToAR(pictureFileName);
            SetIcon(button, Remove);
        }

        private void SetIcon(Button button, int iconId)
        {
            var img = this.CurrentContext.Resources.GetDrawable(iconId);
            
            button.SetCompoundDrawablesWithIntrinsicBounds(null, null, img, null);
        }

        private void SavePictureToAR(string pictureFileName)
        {
            this.SaveAnswer(pictureFileName,
               new AnswerPictureQuestionCommand(interviewId: this.QuestionnairePublicKey, userId: this.Membership.CurrentUser.Id,
                   questionId: this.Model.PublicKey.Id, rosterVector: this.Model.PublicKey.InterviewItemPropagationVector,
                   answerTime: DateTime.UtcNow, pictureFileName: pictureFileName));
        }

        protected bool IsPicturePresent()
        {
            return !string.IsNullOrEmpty(this.Model.AnswerString);
        }

        protected override string GetAnswerStoredInModelAsString()
        {
            return this.Model.AnswerString;
        }

        protected override void PutAnswerStoredInModelToUI()
        {
            if (!IsPicturePresent())
                return;

            var bytes = this.plainInterviewFileStorage.GetInterviewBinaryData(this.QuestionnairePublicKey, Model.AnswerString);
            if (bytes == null || bytes.Length == 0)
            {
                ivImage.SetImageResource(Resource.Drawable.no_image_found);
                return;
            }

            Bitmap bitmap = BitmapFactory.DecodeByteArray(bytes, 0, bytes.Length);
            ivImage.SetImageBitmap(bitmap);
        }
    }
}