using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidApp.ViewModel.QuestionnaireDetails;
using AndroidApp.ViewModel.QuestionnaireDetails.GridItems;
using Cirrious.MvvmCross.Binding.Droid.Interfaces.Views;
using Main.Core.Entities.SubEntities;

namespace AndroidApp.Controls.QuestionnaireDetails.ScreenItems
{
    public class DefaultQuestionViewFactory:IQuestionViewFactory
    {
        #region Implementation of IQuestionViewFactory

        public AbstractQuestionView CreateQuestionView(Context context, QuestionViewModel model)
        {
            var bindingActivity = context as IMvxBindingActivity;
            AbstractQuestionView itemView;
            switch (model.QuestionType)
            {
                case QuestionType.Text:
                    itemView = new TextQuestionView(context, bindingActivity, model);
                    break;
                case QuestionType.Numeric:
                    itemView = new NumericQuestionView(context, bindingActivity, model);
                    break;
                case QuestionType.DateTime:
                    itemView = new DateQuestionView(context, bindingActivity, model);
                    break;
                case QuestionType.SingleOption:
                    itemView = new SingleChoiseQuestionView(context, bindingActivity, model);
                    break;
                case QuestionType.MultyOption:
                    itemView = new MultyQuestionView(context, bindingActivity, model);
                    break;
                default:
                    itemView = new TextQuestionView(context, bindingActivity, model);
                    break;
            }
            return itemView;
        }

        public AbstractQuestionView CreateQuestionView(Context context, RowItem model, HeaderItem header)
        {
            var selectable = header as SelectableHeaderItem;
            QuestionViewModel viewModel;
            if (selectable != null)
                viewModel = new SelectebleQuestionViewModel(model, selectable);
            else
                viewModel = new ValueQuestionViewModel(model, header);

            return CreateQuestionView(context, viewModel);
        }

        #endregion
    }
}