using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf
{
    public class PdfQuestionnaireView : PdfEntityView, IReadSideRepositoryEntity
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

            if (!parentId.HasValue || parentId.Value == this.PublicId)
            {
                this.AddChild(newGroup);
            }
            else
            {
                var pdfGroupView = this.GetGroup(parentId.Value);
                if (pdfGroupView != null)
                    pdfGroupView.AddChild(newGroup);
            }
        }

        internal void AddEntity(PdfEntityView newEntity, Guid? groupPublicKey)
        {
            if (newEntity == null) throw new ArgumentNullException("newEntity");

            if (!groupPublicKey.HasValue)
            {
                throw new ArgumentNullException("groupPublicKey", string.Format("Item {0} can't be created outside group", newEntity.PublicId));
            }

            if (groupPublicKey.Value == this.PublicId)
            {
                this.AddChild(newEntity);
            }
            else
            {
                var group = this.GetGroup(groupPublicKey.Value);
                if (group != null)
                    group.AddChild(newEntity);
            }
        }

        public T GetEntityById<T>(Guid publicKey) where T: PdfEntityView
        {
            return this.Children.TreeToEnumerable().OfType<T>().FirstOrDefault(x => x.PublicId == publicKey);
        }

        public void RemoveEntity(Guid entityId)
        {
            var item = this.Children.TreeToEnumerable().FirstOrDefault(x => x.PublicId == entityId);
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
                        VariableName = childGroup.VariableName,
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
                    var childStaticText = child as IStaticText;
                    if (childQuestion != null)
                    {
                        var newQuestion = new PdfQuestionView
                        {
                            PublicId = childQuestion.PublicKey,
                            Title = childQuestion.QuestionText,
                            Answers = (childQuestion.Answers ?? new List<Answer>()).Select(x => new PdfAnswerView
                            {
                                Title = x.AnswerText,
                                AnswerValue = x.AnswerValue,
                                ParentValue = x.ParentValue,
                            }).ToList(),
                            VariableName = childQuestion.StataExportCaption,
                            ValidationExpression = childQuestion.ValidationExpression,
                            ConditionExpression = childQuestion.ConditionExpression,
                            QuestionType = childQuestion.QuestionType,
                            Depth = this.GetEntityDepth(childQuestion.PublicKey) + 1
                        };

                        this.AddEntity(newQuestion, item.PublicKey);
                    }
                    if (childStaticText != null)
                    {
                        var pdfStaticTextView = new PdfStaticTextView()
                        {
                            PublicId = childStaticText.PublicKey,
                            Title = childStaticText.Text
                        };

                        this.AddEntity(pdfStaticTextView, item.PublicKey);
                    }
                    if (childGroup != null)
                    {
                        var pdfGroupView = new PdfGroupView {
                            PublicId = childGroup.PublicKey, 
                            Title = childGroup.Title, 
                            Depth = this.GetEntityDepth(item.PublicKey) + 1,
                            VariableName = childGroup.VariableName
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