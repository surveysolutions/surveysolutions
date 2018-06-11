using System;
using System.Collections.Generic;
using System.Linq;
using MvvmCross.ViewModels;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Overview
{
    public class OverviewViewModel : MvxViewModel
    {
        private readonly IStatefulInterviewRepository interviewRepository;

        public OverviewViewModel(IStatefulInterviewRepository interviewRepository)
        {
            this.interviewRepository = interviewRepository;
        }

        public void Configure(string interviewId)
        {
            var interview = interviewRepository.Get(interviewId);

            this.Items = interview.GetAllInterviewNodes().Where(x => x.GetType() != typeof(InterviewTreeVariable))
                .Select(this.BuildOverviewNode).ToList();
        }

        private object BuildOverviewNode(IInterviewTreeNode interviewTreeNode)
        {
            if (interviewTreeNode is InterviewTreeQuestion question)
            {
                return new OverviewQuestion
                {
                    Title = question.Title.Text,
                    Answer = question.GetAnswerAsString()
                };
            }

            if (interviewTreeNode is InterviewTreeGroup group)
            {
                return new OverviewGroup
                {
                    Title = group.Title.Text
                };
            }

            if (interviewTreeNode is InterviewTreeStaticText staticText)
            {
                return new OverviewStaticText
                {
                    Title = staticText.Title.Text
                };
            }

            throw new NotSupportedException($"Display of {interviewTreeNode.GetType()} entity is not supported");
        }

        public List<object> Items { get; private set; }
    }
}
