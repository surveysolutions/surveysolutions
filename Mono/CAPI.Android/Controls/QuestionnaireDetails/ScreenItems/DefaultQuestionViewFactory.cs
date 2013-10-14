using System;
using Android.Content;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
using CAPI.Android.Extensions;
using Cirrious.MvvmCross.Binding.BindingContext;
using Main.Core.Entities.SubEntities;

namespace CAPI.Android.Controls.QuestionnaireDetails.ScreenItems
{
    public class DefaultQuestionViewFactory : IQuestionViewFactory
    {
        private readonly IAnswerOnQuestionCommandService commandService;

        public DefaultQuestionViewFactory(IAnswerOnQuestionCommandService commandService)
        {
            this.commandService = commandService;
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
                    itemView = new TextQuestionView(context, bindingActivity, model, questionnairePublicKey, commandService);
                    break;
                case QuestionType.Numeric:
                    var valueQuestionModel = (ValueQuestionViewModel)model;
                        itemView = valueQuestionModel.IsInteger == true
                            ? (AbstractQuestionView)new NumericIntegerQuestionView(context, bindingActivity, model, questionnairePublicKey, commandService)
                            : new NumericRealQuestionView(context, bindingActivity, model, questionnairePublicKey, commandService);
                    break;
                case QuestionType.DateTime:
                    itemView = new DateQuestionView(context, bindingActivity, model, questionnairePublicKey, commandService);
                    break;
                case QuestionType.SingleOption:
                    if (model is LinkedQuestionViewModel)
                        itemView = new SingleOptionLinkedQuestionView(context, bindingActivity, model, questionnairePublicKey,
                            commandService);
                    else
                        itemView = new SingleOptionQuestionView(context, bindingActivity, model, questionnairePublicKey, commandService);
                    break;
                case QuestionType.MultyOption:
                    if (model is LinkedQuestionViewModel)
                        itemView = new MultyOptionLinkedQuestionView(context, bindingActivity, model, questionnairePublicKey,
                            commandService);
                    else
                        itemView = new MultyQuestionView(context, bindingActivity, model, questionnairePublicKey, commandService);
                    break;
                case QuestionType.AutoPropagate:
                    itemView = new AutoPropagateQuestionView(context, bindingActivity, model, questionnairePublicKey, commandService);
                    break;
                case QuestionType.GpsCoordinates:
                    itemView = new GeoPositionQuestionView(context, bindingActivity, model, questionnairePublicKey, commandService);
                    break;
                default:
                    itemView = new TextQuestionView(context, bindingActivity, model, questionnairePublicKey, commandService);
                    break;
            }
            return itemView;
        }

        /*    public AbstractQuestionView CreateQuestionView(Context context, AbstractQuestionRowItem model, HeaderItem header)
            {
                var selectable = header as SelectableHeaderItem;
                QuestionViewModel viewModel;
                if (selectable != null)
                    viewModel = new SelectebleQuestionViewModel(model, selectable);
                else
                    viewModel = new ValueQuestionViewModel(model, header);

                return CreateQuestionView(context, viewModel);
            }*/

        #endregion
    }
}