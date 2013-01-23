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
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Complete;

namespace AndroidApp.EventHandlers
{
    public class CompleteQuestionnaireView
    {
        public CompleteQuestionnaireView()
        {
            Screens = new Dictionary<ItemPublicKey, IQuestionnaireViewModel>();
            Questions=new Dictionary<ItemPublicKey, QuestionViewModel>();
        }
        public void AddScreen(List<ICompleteGroup> rout,
            CompleteQuestionnaireDocument document,
            IEnumerable<QuestionnaireNavigationPanelItem> chapters,
            ICompleteGroup group)
        {
            var key = new ItemPublicKey(group.PublicKey,
                                        group.PropagationPublicKey);

           
            if (group.Propagated == Propagate.None || group.PropagationPublicKey.HasValue)
            {
                var screen = new QuestionnaireScreenViewModel(document.PublicKey, group.Title, document.Title,
                                                         key, BuildItems(group),
                                                         BuildSiblings(rout),
                                                         BuildBreadCrumbs(rout),
                                                         chapters);
                this.Screens.Add(key, screen);
                foreach (var question in screen.Items.OfType<QuestionViewModel>())
                {
                    this.Questions.Add(question.PublicKey, question);
                }
            }
            else
            {
                this.Screens.Add(key, new QuestionnaireGridViewModel(document.PublicKey, group.Title, document.Title,
                                                                     key,
                                                                     BuildSiblings(rout),
                                                                     BuildBreadCrumbs(rout),
                                                                     chapters,
                                                                     group.Children.OfType<ICompleteQuestion>().Select(
                                                                         BuildHeader).ToList(),
                                                                     BuildGridRows(document, group)));
            }
        }


        public IDictionary<ItemPublicKey, IQuestionnaireViewModel> Screens { get; private set; }
        public IDictionary<ItemPublicKey, QuestionViewModel> Questions { get; private set; }


        protected IEnumerable<QuestionnaireNavigationPanelItem> BuildBreadCrumbs(IList<ICompleteGroup> rout)
        {
            return
                rout.Skip(1).ToList().Select(BuildNamivgationItem);
        }
        protected IList<QuestionnaireNavigationPanelItem> BuildSiblings(IList<ICompleteGroup> rout)
        {
            return rout[rout.Count - 2].Children.OfType<ICompleteGroup>().Select(BuildNamivgationItem).ToList();
        }

        protected IEnumerable<QuestionnaireNavigationPanelItem> BuildChapters(CompleteQuestionnaireDocument root)
        {
            return
                root.Children.OfType<ICompleteGroup>().Select(BuildNamivgationItem);
        }
        protected QuestionnaireNavigationPanelItem BuildNamivgationItem(ICompleteGroup g)
        {
            return new QuestionnaireNavigationPanelItem(new ItemPublicKey(g.PublicKey, null), g.Title, 0, 0);
        }

        protected HeaderItem BuildHeader(ICompleteQuestion question)
        {
            /*  var newType = CalculateViewType(question.QuestionType);
              if (!IsTypeSelectable(newType))*/
            return new HeaderItem(question.PublicKey, question.QuestionText, question.Instructions);
            /*  return new SelectableHeaderItem(question.PublicKey, question.QuestionText, question.Instructions,
                                              question.Answers.OfType<ICompleteAnswer>().Select(
                                                  a =>
                                                  new AnswerViewModel(a.PublicKey, a.AnswerText, a.Selected)));*/
        }
        protected IEnumerable<RosterItem> BuildGridRows(CompleteQuestionnaireDocument root, ICompleteGroup template)
        {
            return
                root.Find<ICompleteGroup>(g => g.PublicKey == template.PublicKey && g.PropagationPublicKey.HasValue).
                    Select(
                        g =>
                        new RosterItem(new ItemPublicKey(g.PublicKey, g.PropagationPublicKey.Value), g.Title, BuildItems(g).ToList())
                /* g.Children.OfType<ICompleteQuestion>().Select(
                     q => CreateRowItem(q, g.PropagationPublicKey.Value)).ToList())*/);
        }
        protected IEnumerable<IQuestionnaireItemViewModel> BuildItems(ICompleteGroup screen)
        {
            return screen.Children.Select(CreateView).Where(c => c != null);
        }
        protected IQuestionnaireItemViewModel CreateView(IComposite item)
        {
            var question = item as ICompleteQuestion;

            if (question != null)
            {
                var newType = CalculateViewType(question.QuestionType);
                if (!IsTypeSelectable(newType))
                    return new ValueQuestionViewModel(new ItemPublicKey(question.PublicKey, question.PropagationPublicKey), question.QuestionText,
                                                      newType,
                                                      question.GetAnswerString(),
                                                      question.Enabled, question.Instructions, question.Comments,
                                                      question.Valid, question.Mandatory);
                else
                    return new SelectebleQuestionViewModel(new ItemPublicKey(question.PublicKey, question.PropagationPublicKey), question.QuestionText,
                                                           newType,
                                                           question.Answers.OfType<ICompleteAnswer>().Select(
                                                               a =>
                                                               new AnswerViewModel(a.PublicKey, a.AnswerText, a.Selected)),
                                                           question.Enabled, question.Instructions, question.Comments,
                                                           question.Valid, question.Mandatory, question.GetAnswerString());
            }
            var group = item as ICompleteGroup;
            if (group != null && !group.PropagationPublicKey.HasValue)
                return new GroupViewModel(new ItemPublicKey(group.PublicKey, group.PropagationPublicKey), group.Title,
                                          group.Enabled);
            return null;
        }
        protected QuestionType CalculateViewType(QuestionType type)
        {
            if (type == QuestionType.Numeric || type == QuestionType.AutoPropagate)
                return QuestionType.Numeric;
            if (type == QuestionType.Text || type == QuestionType.Percentage || type == QuestionType.Text)
                return QuestionType.Text;
            if (type == QuestionType.SingleOption || type == QuestionType.DropDownList || type == QuestionType.YesNo)
                return QuestionType.SingleOption;
            return type;
        }

        protected bool IsTypeSelectable(QuestionType type)
        {
            if (type == QuestionType.SingleOption || type == QuestionType.MultyOption)
                return true;
            return false;
        }
    }
}