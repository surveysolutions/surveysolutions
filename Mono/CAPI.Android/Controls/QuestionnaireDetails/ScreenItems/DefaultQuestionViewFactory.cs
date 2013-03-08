using System;
using Android.Content;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
using Cirrious.MvvmCross.Binding.Droid.Interfaces.Views;
using Main.Core.Entities.SubEntities;

namespace CAPI.Android.Controls.QuestionnaireDetails.ScreenItems
{
    public class DefaultQuestionViewFactory:IQuestionViewFactory
    {
        #region Implementation of IQuestionViewFactory

        public AbstractQuestionView CreateQuestionView(Context context, QuestionViewModel model, Guid questionnairePublicKey)
        {
            var bindingActivity = context as IMvxBindingActivity;
            AbstractQuestionView itemView;
            switch (model.QuestionType)
            {
                case QuestionType.Text:
                    itemView = new TextQuestionView(context, bindingActivity, model, questionnairePublicKey);
                    break;
                case QuestionType.Numeric:
                    itemView = new NumericQuestionView(context, bindingActivity, model, questionnairePublicKey);
                    break;
                case QuestionType.DateTime:
                    itemView = new DateQuestionView(context, bindingActivity, model, questionnairePublicKey);
                    break;
                case QuestionType.SingleOption:
                    itemView = new SingleChoiseQuestionView(context, bindingActivity, model, questionnairePublicKey);
                    break;
                case QuestionType.MultyOption:
                    itemView = new MultyQuestionView(context, bindingActivity, model, questionnairePublicKey);
                    break;
                default:
                    itemView = new TextQuestionView(context, bindingActivity, model, questionnairePublicKey);
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