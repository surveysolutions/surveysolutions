using System;
using Android.Content;
using Main.Core.Entities.SubEntities;
using Ncqrs.Commanding.ServiceModel;
using Ninject;
using WB.Core.BoundedContexts.Capi;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.UI.Capi.Shared.Extensions;

namespace WB.UI.Capi.Shared.Controls.ScreenItems
{
    public class DefaultQuestionViewFactory : IQuestionViewFactory
    {
        private readonly IAnswerOnQuestionCommandService answerCommandService;
        private readonly ICommandService commandService;
        private readonly IAuthentication membership;

        public DefaultQuestionViewFactory(IKernel kernel)
        {
            this.answerCommandService = kernel.Get<IAnswerOnQuestionCommandService>();
            this.membership = kernel.Get<IAuthentication>();
            this.commandService = kernel.Get<ICommandService>();
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
                default:
                    itemView = new TextQuestionView(context, bindingActivity, model, questionnairePublicKey, this.commandService, this.answerCommandService, this.membership);
                    break;
            }
            return itemView;
        }

        #endregion
    }
}