﻿using System;
using Ncqrs.Commanding;
using WB.Core.Infrastructure.CommandBus;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Code.CommandTransformation
{
    internal class IntreviewCommand : ICommand
    {
        public Guid InterviewId { get; set; }
        public Guid UserId { get; set; }
        public Guid CommandIdentifier { get; set; }
        public long? KnownVersion { get; set; }
    }
}