// -----------------------------------------------------------------------
// <copyright file="QuestionnaireScreenViewFactory.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using AndroidApp.EventHandlers;
using AndroidApp.ViewModel.QuestionnaireDetails.GridItems;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Complete;
using Main.Core.View;
using Main.DenormalizerStorage;
using Newtonsoft.Json;

namespace AndroidApp.ViewModel.QuestionnaireDetails
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class QuestionnaireScreenViewFactory : IViewFactory<QuestionnaireScreenInput, IQuestionnaireViewModel>
    {
        private readonly IDenormalizerStorage<CompleteQuestionnaireView> _documentStorage;

        public QuestionnaireScreenViewFactory(IDenormalizerStorage<CompleteQuestionnaireView> documentStorage)
        {
            this._documentStorage = documentStorage;
        }

        #region Implementation of IViewFactory<QuestionnaireScreenInput,QuestionnaireScreenViewModel>

        public IQuestionnaireViewModel Load(QuestionnaireScreenInput input)
        {
            var doc = this._documentStorage.Query().First();
            return input.ScreenPublicKey.HasValue ? doc.Screens[input.ScreenPublicKey.Value] : doc.Screens.FirstOrDefault().Value;
            /* ICompleteGroup screen = null;
            IList<QuestionnaireNavigationPanelItem> siblings = new List<QuestionnaireNavigationPanelItem>(0);
            if (!input.ScreenPublicKey.HasValue)
            {
                screen = root.Children.OfType<ICompleteGroup>().First();
                siblings =
                    root.Children.OfType<ICompleteGroup>().Select(
                        g => new QuestionnaireNavigationPanelItem(new ItemPublicKey(g.PublicKey, null), g.Title, 0, 0)).
                        ToList();
            }
            else
            {
                Queue<ICompleteGroup> groups = new Queue<ICompleteGroup>(new ICompleteGroup[] {root});
                while (groups.Count > 0)
                {
                    var current = groups.Dequeue();
                    var possibleScreen =
                        current.Children.OfType<ICompleteGroup>().FirstOrDefault(
                            g =>
                            g.PublicKey == input.ScreenPublicKey.Value.PublicKey &&
                            ((!g.PropagationPublicKey.HasValue && !input.ScreenPublicKey.Value.PropagationKey.HasValue) ||
                             (g.PropagationPublicKey == input.ScreenPublicKey.Value.PropagationKey)));
                    if (possibleScreen != null)
                    {
                        screen = possibleScreen;
                        if (!screen.PropagationPublicKey.HasValue)
                        {
                            if (current == root)
                                siblings =
                                    root.Children.OfType<ICompleteGroup>().Select(
                                        g => new QuestionnaireNavigationPanelItem(new ItemPublicKey(g.PublicKey, null), g.Title, 0, 0)).
                                        ToList();
                            else

                                siblings = new QuestionnaireNavigationPanelItem[]
                                    {
                                        new QuestionnaireNavigationPanelItem(new ItemPublicKey(screen.PublicKey,
                                                                             screen.PropagationPublicKey),
                                                                             screen.Title, 0, 0)
                                    };
                        }
                        else
                        {
                            siblings =
                                current.Children.OfType<ICompleteGroup>().Where(
                                    c => c.PublicKey == input.ScreenPublicKey.Value.PublicKey && c.PropagationPublicKey.HasValue).Select
                                    (g => new QuestionnaireNavigationPanelItem(new ItemPublicKey(g.PublicKey, g.PropagationPublicKey),
                                                                               g.Title, 0, 0)).ToList();
                        }
                        break;
                    }
                    foreach (ICompleteGroup completeGroup in current.Children.OfType<ICompleteGroup>())
                    {
                        groups.Enqueue(completeGroup);
                    }

                }

            }
            if (screen == null)
                throw new ArgumentException("screen cant be found");
            if (screen.Propagated == Propagate.None || screen.PropagationPublicKey.HasValue)
                return new QuestionnaireScreenViewModel(input.QuestionnaireId, screen.Title, root.Title, new ItemPublicKey(screen.PublicKey,
                                                        screen.PropagationPublicKey), BuildItems(screen), siblings,
                                                        BuildBreadCrumbs(root, screen),
                                                        BuildChapters(root));

            return new QuestionnaireGridViewModel(input.QuestionnaireId, screen.Title, root.Title, new ItemPublicKey(screen.PublicKey,null), siblings,
                                                  BuildBreadCrumbs(root,screen),
                                                  BuildChapters(root),
                                                  screen.Children.OfType<ICompleteQuestion>().Select(BuildHeader).ToList(),
                                                  BuildGridRows(root, screen));
            */
        }

        #endregion
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
        protected IEnumerable<QuestionnaireNavigationPanelItem> BuildBreadCrumbs(CompleteQuestionnaireDocument doc, ICompleteGroup screen)
        {
            Stack<ICompleteGroup> stack = new Stack<ICompleteGroup>(new ICompleteGroup[1] { doc });
            List<ICompleteGroup> rout = new List<ICompleteGroup>();
            while (stack.Count > 0)
            {
                var current = stack.Pop();

                while (rout.Count > 0 && !rout[rout.Count - 1].Children.Contains(current))
                {
                    rout.RemoveAt(rout.Count - 1);
                }

                rout.Add(current);
                if (current.PublicKey == screen.PublicKey)
                {
                    if (!screen.PropagationPublicKey.HasValue ||
                        screen.PropagationPublicKey == current.PropagationPublicKey)
                    {
                       break;
                    }
                }

                foreach (IComposite composite in current.Children)
                {
                    var newGroup = composite as ICompleteGroup;
                    if (newGroup != null)
                        stack.Push(newGroup);
                }
            }
            return rout.Skip(1).Select(r =>
                                       new QuestionnaireNavigationPanelItem(
                                           new ItemPublicKey(r.PublicKey, r.PropagationPublicKey), r.Title, 0, 0));
        }

        protected IEnumerable<QuestionnaireNavigationPanelItem> BuildChapters(CompleteQuestionnaireDocument root)
        {
            return
                root.Children.OfType<ICompleteGroup>().Select(
                    g => new QuestionnaireNavigationPanelItem(new ItemPublicKey(g.PublicKey,null), g.Title, 0, 0));
        }

        protected IEnumerable<RosterItem> BuildGridRows(CompleteQuestionnaireDocument root, ICompleteGroup template)
        {
            return
                root.Find<ICompleteGroup>(g => g.PublicKey == template.PublicKey && g.PropagationPublicKey.HasValue).
                    Select(
                        g =>
                        new RosterItem(new ItemPublicKey(g.PublicKey, g.PropagationPublicKey.Value), g.Title,BuildItems(g).ToList())
                                      /* g.Children.OfType<ICompleteQuestion>().Select(
                                           q => CreateRowItem(q, g.PropagationPublicKey.Value)).ToList())*/);
        }

        protected IEnumerable<IQuestionnaireItemViewModel> BuildItems(ICompleteGroup screen)
        {
            return screen.Children.Select(CreateView).Where(c => c != null);
        }

      /*  protected AbstractRowItem CreateRowItem(ICompleteQuestion item, Guid propagationKey)
        {
            var newType = CalculateViewType(item.QuestionType);
            if (IsTypeSelectable(newType))
                return new SelectableRowItem(item.PublicKey, propagationKey, newType,
                                             item.Enabled, item.Valid, item.Comments, item.GetAnswerString(), item.Answers.OfType<ICompleteAnswer>().Select(
                                                               a =>
                                                               new AnswerViewModel(a.PublicKey, a.AnswerText, a.Selected)));
            else
                return new ValueRowItem(item.PublicKey, propagationKey, item.GetAnswerString(), newType,
                                                  item.Enabled, item.Valid, item.Comments);
        }*/

        protected IQuestionnaireItemViewModel CreateView(IComposite item)
        {
            var question = item as ICompleteQuestion;

            if (question != null)
            {
                var newType = CalculateViewType(question.QuestionType);
                if (!IsTypeSelectable(newType))
                    return new ValueQuestionViewModel(new ItemPublicKey( question.PublicKey,question.PropagationPublicKey), question.QuestionText,
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
