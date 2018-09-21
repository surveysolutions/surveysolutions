using System;
using System.Collections.Generic;
using System.Linq;

namespace WB.Services.Export.Questionnaire
{
    public class QuestionnaireDocument : Group
    {
      //  private bool childrenWereConnected = false;

        public QuestionnaireDocument(List<IQuestionnaireEntity> children = null) : base(children)
        {
        }

        public string Id { get; set;}

        public override void ConnectChildrenWithParent()
        {
            //if (childrenWereConnected) return;

            base.ConnectChildrenWithParent();

          //  childrenWereConnected = true;
        }

        public bool IsIntegerQuestion(Guid publicKey)
        {
            var result = this.Find<NumericQuestion>(x => x.PublicKey == publicKey && x.QuestionType == QuestionType.Numeric && x.IsInteger);
            return result != null;
        }

        public string GetQuestionVariableName(Guid publicKey)
        {
            var result = this.FirstOrDefault<Question>(x => x.PublicKey == publicKey);
            return result?.VariableName;
        }

        public IEnumerable<Guid> GetRostersFromTopToSpecifiedEntity(Guid entityId)
        {
            return this
                .GetAllParentGroupsForEntityStartingFromBottom(entityId)
                .Where(this.IsRosterGroup)
                .Reverse()
                .ToList();
        }

        private bool IsRosterGroup(Guid groupId)
        {
            Group group = this.Find<Group>(groupId);

            if (group == null) return false;

            return group.IsRoster;
        }

        private IEnumerable<Guid> GetAllParentGroupsForEntityStartingFromBottom(Guid entityId)
        {
            IQuestionnaireEntity entity = this.Find<IQuestionnaireEntity>(entityId);

            var parentGroup = (Group)entity.GetParent();

            return this.GetSpecifiedGroupAndAllItsParentGroupsStartingFromBottom(parentGroup);
        }

        private IEnumerable<Guid> GetSpecifiedGroupAndAllItsParentGroupsStartingFromBottom(Group group)
        {
            return this.GetSpecifiedGroupAndAllItsParentGroupsStartingFromBottom(group, this).Select(_ => _.PublicKey);
        }

        private IEnumerable<Group> GetSpecifiedGroupAndAllItsParentGroupsStartingFromBottom(Group group, QuestionnaireDocument document)
        {
            while (group != document)
            {
                yield return group;
                group = (Group)group.GetParent();
            }
        }

        public string GetRosterVariableName(Guid publicKey)
        {
            Group group = this.Find<Group>(publicKey);
            return group?.VariableName;
        }

        public string GetValidationMessage(Guid publicKey, int conditionIndex)
        {
            var entity = this.Find<IValidatableQuestionnaireEntity>(publicKey);
            return entity.ValidationConditions[conditionIndex].Message;
        }

        public Guid[] GetRosterSizeSourcesForEntity(Guid entityId)
        {
            var entity = this.Find<IQuestionnaireEntity>(entityId);


            var rosterSizes = new List<Guid>();

            while (entity != this && entity != null)
            {
                if (entity is Group group)
                {
                    if (group.IsRoster)
                        rosterSizes.Insert(0, this.GetRosterSource(group.PublicKey));
                }

                entity = entity.GetParent();
            }

            return rosterSizes.ToArray();
        }

        private Guid GetRosterSource(Guid rosterId)
        {
            Group roster = this.Find<Group>(rosterId);

            return roster.RosterSizeQuestionId ?? roster.PublicKey;
        }
    }
}
