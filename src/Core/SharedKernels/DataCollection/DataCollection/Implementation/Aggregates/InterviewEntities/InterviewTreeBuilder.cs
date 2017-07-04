using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Services;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities
{
    public class InterviewTreeBuilder : IInterviewTreeBuilder
    {
        private readonly ISubstitutionTextFactory substitutionTextFactory;

        public InterviewTreeBuilder(ISubstitutionTextFactory substitutionTextFactory)
        {
            this.substitutionTextFactory = substitutionTextFactory;
        }

        public InterviewTree BuildInterviewTree(Guid interviewId, IQuestionnaire questionnaire)
        {
            var tree = new InterviewTree(interviewId, questionnaire, this.substitutionTextFactory);
            var sections = this.BuildInterviewTreeSections(tree, questionnaire, this.substitutionTextFactory).ToArray();

            tree.SetSections(sections);
            return tree;
        }

        private IEnumerable<InterviewTreeSection> BuildInterviewTreeSections(InterviewTree tree, IQuestionnaire questionnaire, ISubstitutionTextFactory textFactory)
        {
            var sectionIds = questionnaire.GetAllSections();

            foreach (var sectionId in sectionIds)
            {
                var sectionIdentity = new Identity(sectionId, RosterVector.Empty);
                var section = this.BuildInterviewTreeSection(tree, sectionIdentity, questionnaire, textFactory);

                yield return section;
            }
        }

        private InterviewTreeSection BuildInterviewTreeSection(InterviewTree tree, Identity sectionIdentity, IQuestionnaire questionnaire, ISubstitutionTextFactory textFactory)
        {
            var section = InterviewTree.CreateSection(tree, questionnaire, textFactory, sectionIdentity);

            section.AddChildren(this.BuildInterviewTreeGroupChildren(tree, sectionIdentity, questionnaire, textFactory).ToList());

            return section;
        }

        private InterviewTreeSubSection BuildInterviewTreeSubSection(InterviewTree tree, Identity groupIdentity, IQuestionnaire questionnaire, ISubstitutionTextFactory textFactory)
        {
            var subSection = InterviewTree.CreateSubSection(tree, questionnaire, textFactory, groupIdentity);

            subSection.AddChildren(this.BuildInterviewTreeGroupChildren(tree, groupIdentity, questionnaire, textFactory).ToList());

            return subSection;
        }

        private IEnumerable<IInterviewTreeNode> BuildInterviewTreeGroupChildren(InterviewTree tree, Identity groupIdentity, IQuestionnaire questionnaire, ISubstitutionTextFactory textFactory)
        {
            var childIds = questionnaire.GetChildEntityIds(groupIdentity.Id);

            foreach (var childId in childIds)
            {
                Identity entityIdentity = new Identity(childId, groupIdentity.RosterVector);

                if (questionnaire.IsRosterGroup(childId)) continue;

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

    public interface IInterviewTreeBuilder
    {
        InterviewTree BuildInterviewTree(Guid interviewId, IQuestionnaire questionnaire);
    }
}
