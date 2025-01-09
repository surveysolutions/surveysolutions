using System;
using MvvmCross.Plugin.Messenger;

namespace WB.Core.SharedKernels.Enumerator.ViewModels
{
    public class DashboardChangedMessage : MvxMessage
    {
        public DashboardChangedMessage(object sender) : base(sender)
        {
        }
        
        public int? AssignmentId { get; set; }
        public Guid? InterviewId { get; set; }
    }
}
