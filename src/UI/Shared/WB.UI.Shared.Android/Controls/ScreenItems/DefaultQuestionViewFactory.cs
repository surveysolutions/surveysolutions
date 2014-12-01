using System;
using Android.Content;
using Cirrious.CrossCore;
using Cirrious.CrossCore.Core;
using Cirrious.MvvmCross.Droid.Platform;
using Main.Core.Entities.SubEntities;

using Ninject;
using WB.Core.BoundedContexts.Capi;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.UI.Shared.Android.Extensions;

namespace WB.UI.Shared.Android.Controls.ScreenItems
{
    public class DefaultQuestionViewFactory : IQuestionViewFactory
    {
        private readonly IAnswerOnQuestionCommandService answerCommandService;
        private readonly ICommandService commandService;
        private readonly IAuthentication membership;
        private readonly IPlainInterviewFileStorage plainInterviewFileStorage;

        public DefaultQuestionViewFactory(IKernel kernel)
        {
            this.answerCommandService = kernel.Get<IAnswerOnQuestionCommandService>();
            this.membership = kernel.Get<IAuthentication>();
            this.commandService = kernel.Get<ICommandService>();
            this.plainInterviewFileStorage = kernel.Get<IPlainInterviewFileStorage>();
        }

        #region Implementation of IQuestionViewFactory

        public AbstractQuestionView CreateQuestionView(Context context, QuestionViewModel model,
            Guid questionnairePublicKey)
        {
            var bindingActivity = context.ToBindingContext();

            AbstractQuestionView itemView;
            switch (model.QuestionType)
            {
                case QuestionType.Text:
                    itemView = new TextQuestionView(context, bindingActivity, model, questionnairePublicKey,this.commandService, this.answerCommandService, this.membership);
                    break;
                case QuestionType.QRBarcode:
                    itemView = new QRBarcodeQuestionView(context, bindingActivity, model, questionnairePublicKey, this.commandService, this.answerCommandService, this.membership);
                    break;
                case QuestionType.TextList:
                    itemView = new TextListQuestionView(context, bindingActivity, model, questionnairePublicKey, this.commandService, this.answerCommandService, this.membership);
                    break;

                case QuestionType.Numeric:
                    var valueQuestionModel = (ValueQuestionViewModel)model;
                        itemView = valueQuestionModel.IsInteger == true
                            ? (AbstractQuestionView)new NumericIntegerQuestionView(context, bindingActivity, model, questionnairePublicKey, this.commandService, this.answerCommandService, this.membership)
                            : new NumericRealQuestionView(context, bindingActivity, model, questionnairePublicKey, this.commandService, this.answerCommandService, this.membership);
                    break;
                case QuestionType.DateTime:
                    itemView = new DateQuestionView(context, bindingActivity, model, questionnairePublicKey, this.commandService, this.answerCommandService, this.membership);
                    break;
                case QuestionType.SingleOption:
                    if (model is LinkedQuestionViewModel)
                        itemView = new SingleOptionLinkedQuestionView(context, bindingActivity, model, questionnairePublicKey,
                            this.commandService, this.answerCommandService, this.membership);
                    else if(model is FilteredComboboxQuestionViewModel)
                        itemView = new FilteredComboboxQuestionView(context, bindingActivity, model, questionnairePublicKey,
                            this.commandService, this.answerCommandService, this.membership);
                    else if (model is CascadingComboboxQuestionViewModel)
                        itemView = new CascadingComboboxQuestionView(context, bindingActivity, model, questionnairePublicKey,
                            this.commandService, this.answerCommandService, this.membership);
                    else
                        itemView = new SingleOptionQuestionView(context, bindingActivity, model, questionnairePublicKey, this.commandService, this.answerCommandService, this.membership);
                    break;
                case QuestionType.MultyOption:
                    if (model is LinkedQuestionViewModel)
                        itemView = new MultyOptionLinkedQuestionView(context, bindingActivity, model, questionnairePublicKey, this.commandService, this.answerCommandService, this.membership);
                    else
                        itemView = new MultyQuestionView(context, bindingActivity, model, questionnairePublicKey, this.commandService, this.answerCommandService, this.membership);
                    break;
                case QuestionType.AutoPropagate:
                    itemView = new AutoPropagateQuestionView(context, bindingActivity, model, questionnairePublicKey, this.commandService, this.answerCommandService, this.membership);
                    break;
                case QuestionType.GpsCoordinates:
                    itemView = new GeoPositionQuestionView(context, bindingActivity, model, questionnairePublicKey, this.commandService, this.answerCommandService, this.membership);
                    break;
                case QuestionType.Multimedia:
                    itemView = new PictureQuestionView(context, bindingActivity, model, questionnairePublicKey, this.commandService,
                        this.answerCommandService, this.membership, this.plainInterviewFileStorage);
                    break;
                default:
                    itemView = new TextQuestionView(context, bindingActivity, model, questionnairePublicKey, this.commandService, this.answerCommandService, this.membership);
                    break;
            }
            return itemView;
        }

        #endregion
    }
}