using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Net;
using Android.Provider;
using Android.Runtime;
using Android.Text;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Cirrious.MvvmCross.Plugins.PictureChooser;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.BoundedContexts.Capi;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.UI.Shared.Android.Controls.MaskedEditTextControl;
using WB.UI.Shared.Android.Helpers;
using File = Java.IO.File;
using Uri = Android.Net.Uri;

namespace WB.UI.Shared.Android.Controls.ScreenItems
{
    public class PictureQuestionView : AbstractQuestionView
    {
        public PictureQuestionView(Context context, IMvxAndroidBindingContext bindingActivity, QuestionViewModel source,
                                Guid questionnairePublicKey, ICommandService commandService,
                                IAnswerOnQuestionCommandService answerCommandService, IAuthentication membership, IPlainFileRepository plainFileRepository)
            : base(context, bindingActivity, source, questionnairePublicKey, commandService, answerCommandService, membership)
        {
            this.plainFileRepository = plainFileRepository;

            if (IsThereAnAppToTakePictures())
            {
                this.pictureChooserTask = Mvx.Resolve<IMvxPictureChooserTask>();
                this.btnTakePicture = new Button(this.Context);
                this.btnTakePicture.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.FillParent,
                    ViewGroup.LayoutParams.WrapContent);
                this.btnTakePicture.Text = "Take Picture";
                this.btnTakePicture.Click += this.BtnTakePictureClick;

                this.btnRemovePicture = new Button(this.Context);
                this.btnRemovePicture.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.FillParent,
                    ViewGroup.LayoutParams.WrapContent);
                this.btnRemovePicture.Text = "Remove Picture";
                this.btnRemovePicture.Click += this.BtnRemovePictureClick;

                ivImage = new ImageView(this.Context);
                ivImage.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.FillParent, ViewGroup.LayoutParams.FillParent);

                this.llWrapper.AddView(this.btnTakePicture);
                this.llWrapper.AddView(this.btnRemovePicture);
                this.llWrapper.AddView(this.ivImage);

                this.PutAnswerStoredInModelToUI();
            }
            else
            {
                var tvWarning = new TextView(this.Context);

                tvWarning.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.WrapContent,
                    ViewGroup.LayoutParams.WrapContent);

                tvWarning.Text = "Camera is absent";

                this.llWrapper.AddView(tvWarning);
            }
        }

        private readonly IPlainFileRepository plainFileRepository;
        private readonly IMvxPictureChooserTask pictureChooserTask;
        protected readonly Button btnTakePicture;
        protected readonly Button btnRemovePicture;
        protected readonly ImageView ivImage;

        protected ValueQuestionViewModel TypedMode
        {
            get { return this.Model as ValueQuestionViewModel; }
        }

        protected bool IsThereAnAppToTakePictures()
        {
            Intent intent = new Intent(MediaStore.ActionImageCapture);
            IList<ResolveInfo> availableActivities = Context.PackageManager.QueryIntentActivities(intent, PackageInfoFlags.MatchDefaultOnly);
            return availableActivities != null && availableActivities.Count > 0;
        }

        protected void BtnTakePictureClick(object sender, EventArgs e)
        {
            pictureChooserTask.TakePicture(400, 95, OnPicture, () => { });
        }

        private void BtnRemovePictureClick(object sender, EventArgs e)
        {
            plainFileRepository.RemoveInterviewBinaryData(this.QuestionnairePublicKey, Model.AnswerString);
         //   ivImage.SetImageResource(0);
            ivImage.SetImageDrawable(null);
            this.SavePictureToAR(string.Empty);
        }

        private void OnPicture(Stream pictureStream)
        {
            var pictureFileName = String.Format("{0}{1}.jpg", Model.Variable,
                string.Join("-", Model.PublicKey.InterviewItemPropagationVector));
            byte[] data = null;
            using (var memoryStream = new MemoryStream())
            {
                pictureStream.CopyTo(memoryStream);
                data = memoryStream.ToArray();
                plainFileRepository.StoreInterviewBinaryData(this.QuestionnairePublicKey, pictureFileName, data);
            }

            Bitmap bitmap = BitmapFactory.DecodeByteArray(data, 0, data.Length);
            ivImage.SetImageBitmap(bitmap);
            this.SavePictureToAR(pictureFileName);
        }

        private void SavePictureToAR(string pictureFileName)
        {
            this.SaveAnswer(pictureFileName,
               new AnswerPictureQuestionCommand(interviewId: this.QuestionnairePublicKey, userId: this.Membership.CurrentUser.Id,
                   questionId: this.Model.PublicKey.Id, rosterVector: this.Model.PublicKey.InterviewItemPropagationVector,
                   answerTime: DateTime.UtcNow, pictureFileName: pictureFileName));
        }

        protected override string GetAnswerStoredInModelAsString()
        {
            return this.Model.AnswerString;
        }

        protected override void PutAnswerStoredInModelToUI()
        {
            var bytes = plainFileRepository.GetInterviewBinaryData(this.QuestionnairePublicKey, Model.AnswerString);
            if (bytes == null || bytes.Length == 0)
            {
               // ivImage.SetImageResource(0);
                ivImage.SetImageDrawable(null);
                return;
            }

            Bitmap bitmap = BitmapFactory.DecodeByteArray(bytes, 0, bytes.Length);
            ivImage.SetImageBitmap(bitmap);
        }
    }
}