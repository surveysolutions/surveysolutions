using System;
using System.Collections.Generic;
using System.Linq;
using MvvmCross.ViewModels;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Properties;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Overview
{
    public class OverviewViewModel : MvxViewModel
    {
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly DynamicTextViewModel nameViewModel;

        public OverviewViewModel(IStatefulInterviewRepository interviewRepository,
            DynamicTextViewModel nameViewModel)
        {
            this.interviewRepository = interviewRepository;
            this.nameViewModel = nameViewModel;
        }

        public void Configure(string interviewId)
        {
            var interview = interviewRepository.Get(interviewId);
            var sections = interview.GetEnabledSections().Select(x => x.Identity);

            this.Items = interview.GetUnderlyingInterviewerEntities()
                .Select(x => this.BuildOverviewNode(x, interview, sections))
                .ToList();

            this.Name = nameViewModel;
            this.Name.InitAsStatic(UIResources.Interview_Overview_Name);
        }

        private OverviewNode BuildOverviewNode(Identity interviewerEntityIdentity, IStatefulInterview interview,
            IEnumerable<Identity> sections)
        {
            var question = interview.GetQuestion(interviewerEntityIdentity);

            if (question != null)
            {
                if (question.IsAnswered())
                {
                    return new OverviewQuestion
                    {
                        Id = question.Identity.ToString(),
                        Title = question.Title.Text,
                        Answer = question.GetAnswerAsString()
                    };
                }
                else
                {
                    return new OverviewQuestionUnaswered
                    {
                        Id = question.Identity.ToString(),
                        Title = question.Title.Text
                    };
                }
            }

            var staticText = interview.GetStaticText(interviewerEntityIdentity);
            if (staticText != null)
            {
                return new OverviewStaticText
                {
                    Id = staticText.Identity.ToString(),
                    Title = staticText.Title.Text
                };
            }

            var group = interview.GetGroup(interviewerEntityIdentity);
            if (group != null)
            {
                if (sections.Any(x => x.Equals(group.Identity)))
                {
                    return new OverviewSection
                    {
                        Id = group.Identity.ToString(),
                        Title = group.Title.Text
                    };
                }

                return new OverviewGroup
                {
                    Id = group.Identity.ToString(),
                    Title = group.Title.Text
                };
            }

            throw new NotSupportedException($"Display of {interviewerEntityIdentity.GetType()} entity is not supported");
        }

        public List<OverviewNode> Items { get; private set; }

        public DynamicTextViewModel Name { get; private set; }
    }
}

