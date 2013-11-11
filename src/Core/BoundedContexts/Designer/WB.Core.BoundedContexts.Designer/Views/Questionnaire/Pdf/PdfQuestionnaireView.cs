using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.Infrastructure.ReadSide;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf
{
    public class PdfQuestionnaireView : PdfEntityView, IView
    {
        public string CreatedBy { get; set; }

        public DateTime CreationDate { get; set; }

        public int GetChaptersCount()
        {
            return this.Children.TreeToEnumerable().OfType<PdfGroupView>().Count(x => x.GetParent().PublicId == this.PublicId);
        }

        public int GetGroupsCount()
        {
            return
                this.Children.TreeToEnumerable().OfType<PdfGroupView>().Count(x => x.GetParent() != null && x.GetParent().PublicId != this.PublicId);
        }

        public int GetQuestionsCount()
        {
            return this.Children.TreeToEnumerable().OfType<PdfQuestionView>().Count();
        }

        public int GetQuestionsWithConditionsCount()
        {
            return this.Children.TreeToEnumerable().OfType<PdfQuestionView>().Count(x => x.GetHasCondition());
        }

        internal void RemoveGroup(Guid groupId)
        {
            var item = this.Children.TreeToEnumerable<PdfEntityView>().FirstOrDefault(x => x.PublicId == groupId);
            if (item != null)
            {
                item.GetParent().Children.Remove(item);
            }
        }

        internal PdfGroupView GetGroup(Guid groupId)
        {
            return this.Children.TreeToEnumerable().OfType<PdfGroupView>().FirstOrDefault(x => x.PublicId == groupId);
        }

        internal int GetEntityDepth(Guid? entityId)
        {
            var item = this.Children.TreeToEnumerable().FirstOrDefault(x => x.PublicId == entityId);
            if (item != null)
            {
                int result = 1;
                var itemParent = item.GetParent();
                while (itemParent != null)
                {
                    itemParent = itemParent.GetParent();
                    result++;
                }

                return result;
            }

            return 0;
        }

        internal void AddGroup(PdfGroupView newGroup, Guid? parentId)
        {
            if (newGroup == null) throw new ArgumentNullException("newGroup");

            if (parentId.HasValue)
            {
                var pdfGroupView = this.GetGroup(parentId.Value);
                pdfGroupView.AddChild(newGroup);
            }
            else
            {
                this.AddChild(newGroup);
            }
        }

        internal void AddQuestion(PdfQuestionView newQuestion, Guid? groupPublicKey)
        {
            if (newQuestion == null) throw new ArgumentNullException("newQuestion");

            if (!groupPublicKey.HasValue)
            {
                throw new ArgumentNullException("groupPublicKey", string.Format("Question {0} cant be created not inside group", newQuestion.PublicId));
            }

            var group = this.GetGroup(groupPublicKey.Value);
            group.AddChild(newQuestion);
        }

        public PdfQuestionView GetQuestion(Guid publicKey)
        {
            return this.Children.TreeToEnumerable().OfType<PdfQuestionView>().FirstOrDefault(x => x.PublicId == publicKey);
        }

        public void RemoveQuestion(Guid questionId)
        {
            var item = this.Children.TreeToEnumerable().FirstOrDefault(x => x.PublicId == questionId);
            if (item != null)
            {
                item.GetParent().Children.Remove(item);
            }
        }

        public void FillFrom(QuestionnaireDocument source)
        {
            var children = source.Children ?? new List<IComposite>();

            foreach (var child in children)
            {
                var childGroup = child as IGroup;
                if (childGroup != null)
                {
                    var pdfGroupView = new PdfGroupView
                    {
                        PublicId = childGroup.PublicKey,
                        Title = childGroup.Title,
                        Depth = 1
                    };
                    this.AddGroup(pdfGroupView, null);
                    Fill(pdfGroupView, child);
                }
            }
        }

        private void Fill(PdfGroupView group, IComposite item)
        {
            if (item.Children != null)
            {
                foreach (var child in item.Children)
                {
                    var childQuestion = child as IQuestion;
                    var childGroup = child as IGroup;
                    if (childQuestion != null)
                    {
                        var newQuestion=new PdfQuestionView
                        {
                            PublicId = childQuestion.PublicKey,
                            Title = childQuestion.QuestionText,
                            Answers = (childQuestion.Answers ?? new List<IAnswer>()).Select(x => new PdfAnswerView
                            {
                                Title = x.AnswerText,
                                AnswerType = x.AnswerType,
                                AnswerValue = x.AnswerValue
                            }).ToList(),
                            Variable = childQuestion.StataExportCaption
                        };

                        newQuestion.ConditionExpression = childQuestion.ConditionExpression;
                        this.AddQuestion(newQuestion, item.PublicKey);
                    }
                    if (childGroup != null)
                    {
                        var pdfGroupView = new PdfGroupView {
                            PublicId = childGroup.PublicKey, 
                            Title = childGroup.Title, 
                            Depth = this.GetEntityDepth(item.PublicKey) + 1
                        };
                        this.AddGroup(pdfGroupView, group.PublicId);
                        Fill(pdfGroupView, child);
                    }
                }
            }
        }

        public IEnumerable<PdfQuestionView> GetQuestionsWithConditions()
        {
            return Children.TreeToEnumerable().OfType<PdfQuestionView>().Where(x => x.GetHasCondition()).OrderBy(x => x.GetStringItemNumber());
        }

        public IEnumerable<PdfQuestionView> GetQuestionsWithValidation()
        {
            return Children.TreeToEnumerable().OfType<PdfQuestionView>().Where(x => !string.IsNullOrEmpty(x.GetReadableValidationExpression())).OrderBy(x => x.GetStringItemNumber());
        }
    }
}