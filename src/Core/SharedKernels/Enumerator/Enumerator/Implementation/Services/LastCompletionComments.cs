using System;
using System.Collections.Generic;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    public class LastCompletionComments : ILastCompletionComments
    {
        private readonly IPlainStorage<InterviewView> interviewViewRepository;

        public LastCompletionComments(IPlainStorage<InterviewView> interviewViewRepository)
        {
            this.interviewViewRepository = interviewViewRepository;
        }

        public void Store(Guid interviewId, string comment)
        {
            var interviewView = interviewViewRepository.GetById(interviewId.FormatGuid());
            interviewView.InterviewComment = comment;
            interviewViewRepository.Store(interviewView);
        }

        public string Get(Guid interviewId)
        {
            var interviewView = interviewViewRepository.GetById(interviewId.FormatGuid());
            return interviewView.InterviewComment;
        }

        public void Remove(Guid interviewId)
        {
            var interviewView = interviewViewRepository.GetById(interviewId.FormatGuid());
            interviewView.InterviewComment = null;
            interviewViewRepository.Store(interviewView);
        }
    }
}
