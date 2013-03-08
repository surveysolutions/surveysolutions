using Android.App;

namespace Mono.Android.Crasher.Data.Submit
{
    public interface IReportSender
    {
        /// <summary>
        /// Basic initialization of report sender
        /// </summary>
        /// <param name="application"><see cref="Application"/> instance being reported.</param>
        void Initialize(Application application);
        /// <summary>
        /// Method tp send builded report
        /// </summary>
        /// <param name="errorContent">Builded crash report data</param>
        void Send(ReportData errorContent);
    }
}