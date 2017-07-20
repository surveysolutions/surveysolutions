using System;
using System.Security.Principal;

namespace WB.UI.Headquarters.API.WebInterview
{
    public interface IWebInterviewAllowService
    {
        void CheckWebInterviewAccessPermissions(string interviewId, Guid? userId);
    }
}