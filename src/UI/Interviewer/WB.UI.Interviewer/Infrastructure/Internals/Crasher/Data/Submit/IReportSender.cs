namespace WB.UI.Interviewer.Infrastructure.Internals.Crasher.Data.Submit
{
    public interface IReportSender
    {
        void Send(ReportData errorContent);
    }
}