using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.SharedKernels.DataCollection.ExpressionStorage;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities
{
    public static class Ext
    {
        public static T To<T>(this object obj)
        {
            Type t = typeof(T);
            Type u = Nullable.GetUnderlyingType(t);

            if (u != null)
            {
                return (obj == null) ? default(T) : (T)Convert.ChangeType(obj, u);
            }
            return (T)Convert.ChangeType(obj, t);
        }
    }
    public class InterviewState : IInterviewState
    {
        private readonly InterviewTree tree;

        public InterviewState(InterviewTree tree)
        {
            this.tree = tree;
        }

        public T GetAnswer<T>(Guid questionId, IEnumerable<int> rosterVector)
        {
            var question = tree.GetQuestion(questionId, new RosterVector(rosterVector));

            if (!question.IsAnswered() || question.IsDisabled())
                return default(T);

            if (question.IsText) return question.AsText.GetAnswer().Value.To<T>();
            if (question.IsMultimedia) return question.AsMultimedia.GetAnswer().FileName.To<T>();
            if (question.IsQRBarcode) return question.AsQRBarcode.GetAnswer().DecodedText.To<T>();
            if (question.IsInteger) return question.AsInteger.GetAnswer().Value.To<T>();
            if (question.IsDouble) return question.AsDouble.GetAnswer().Value.To<T>();
            if (question.IsDateTime) return question.AsDateTime.GetAnswer().Value.To<T>();
            if (question.IsGps) return question.AsGps.GetAnswer().Value.To<T>();
            if (question.IsTextList) return question.AsTextList.GetAnswer().ToTupleArray().To<T>() ;
            if (question.IsSingleLinkedOption) return question.AsSingleLinkedOption.GetAnswer().SelectedValue.To<T>();
            if (question.IsMultiLinkedOption) return question.AsMultiLinkedOption.GetAnswer().CheckedValues.To<T>();
            if (question.IsSingleFixedOption) return question.AsSingleFixedOption.GetAnswer().SelectedValue.To<T>();
            if (question.IsMultiFixedOption) return question.AsMultiFixedOption.GetAnswer().ToDecimals().ToArray().To<T>();
            if (question.IsYesNo) return question.AsYesNo.GetAnswer().ToYesNoAnswers().To<T>();
            if (question.IsSingleLinkedToList) return question.AsSingleLinkedToList.GetAnswer().SelectedValue.To<T>();
            if (question.IsMultiLinkedToList) return question.AsMultiLinkedToList.GetAnswer().ToDecimals().To<T>();

            return default(T);
        }

        public IEnumerable<Identity> FindEntitiesFromSameOrDeeperLevel(Guid entityIdToSearch, Identity startingSearchPointIdentity)
        {
            return this.tree.FindEntitiesFromSameOrDeeperLevel(entityIdToSearch, startingSearchPointIdentity);
        }

        public int GetRosterIndex(Identity rosterIdentity)
        {
            return this.tree.GetRoster(rosterIdentity).SortIndex;
        }

        public string GetRosterTitle(Identity rosterIdentity)
        {
            return this.tree.GetRoster(rosterIdentity).RosterTitle;
        }
    }
}
