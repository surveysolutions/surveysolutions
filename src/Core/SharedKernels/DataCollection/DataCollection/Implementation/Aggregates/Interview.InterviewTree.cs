using System.Collections.Generic;
using System.Linq;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Services;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    public partial class Interview
    {
        protected InterviewTree BuildInterviewTree(IQuestionnaire questionnaire)
        {
            var tree = new InterviewTree(this.EventSourceId, questionnaire, this.substitionTextFactory);
            var sections = this.BuildInterviewTreeSections(tree, questionnaire, this.substitionTextFactory).ToList();

            tree.SetSections(sections);
            return tree;
        }

        private IEnumerable<InterviewTreeSection> BuildInterviewTreeSections(InterviewTree tree, IQuestionnaire questionnaire, ISubstitionTextFactory textFactory)
        {
            var sectionIds = questionnaire.GetAllSections();

            foreach (var sectionId in sectionIds)
            {
                var sectionIdentity = new Identity(sectionId, RosterVector.Empty);
                var section = this.BuildInterviewTreeSection(tree, sectionIdentity, questionnaire, textFactory);

                yield return section;
            }
        }

        private InterviewTreeSection BuildInterviewTreeSection(InterviewTree tree, Identity sectionIdentity, IQuestionnaire questionnaire, ISubstitionTextFactory textFactory)
        {
            var section = InterviewTree.CreateSection(tree, questionnaire, textFactory, sectionIdentity);

            section.AddChildren(this.BuildInterviewTreeGroupChildren(tree, sectionIdentity, questionnaire, textFactory).ToList());
            
            return section;
        }

        private InterviewTreeSubSection BuildInterviewTreeSubSection(InterviewTree tree, Identity groupIdentity, IQuestionnaire questionnaire, ISubstitionTextFactory textFactory)
        {
            var subSection = InterviewTree.CreateSubSection(tree, questionnaire, textFactory, groupIdentity);

            subSection.AddChildren(this.BuildInterviewTreeGroupChildren(tree, groupIdentity, questionnaire, textFactory).ToList());

            return subSection;
        }

        private IEnumerable<IInterviewTreeNode> BuildInterviewTreeGroupChildren(InterviewTree tree, Identity groupIdentity, IQuestionnaire questionnaire, ISubstitionTextFactory textFactory)
        {
            var childIds = questionnaire.GetChildEntityIds(groupIdentity.Id);

            foreach (var childId in childIds)
            {
                Identity entityIdentity = new Identity(childId, groupIdentity.RosterVector);

                if(questionnaire.IsRosterGroup(childId)) continue;

                if (questionnaire.HasGroup(childId))
                    yield return this.BuildInterviewTreeSubSection(tree, entityIdentity, questionnaire, textFactory);
                else if (questionnaire.HasQuestion(childId))
                    yield return InterviewTree.CreateQuestion(tree, questionnaire, textFactory, entityIdentity);
                else if (questionnaire.IsStaticText(childId))
                    yield return InterviewTree.CreateStaticText(tree, questionnaire, textFactory, entityIdentity);
                else if (questionnaire.IsVariable(childId))
                    yield return InterviewTree.CreateVariable(entityIdentity);
                
            }
        }
    }
}
