using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Content;
using Android.Runtime;
using Android.Text.Format;
using Android.Util;
using Java.Lang;
using Mono.Android.Crasher.Data;
using Mono.Android.Crasher.Data.Submit;
using Mono.Android.Crasher.Utils;
using Exception = System.Exception;


namespace Mono.Android.Crasher
{
    /// <summary>
    /// Proccess exceptions by building and sending reports.
    /// </summary>
    class ExceptionProcessor : IDisposable
    {
        private readonly Context _context;
        private readonly ReportField[] _reportFields;
        private readonly List<IReportSender> _reportSenders = new List<IReportSender>();
        private readonly InteractionMode _interactionMode;
        private readonly Time _appStartDate;
        private readonly string _initialConfiguration;
        private readonly IList<ICustomReportDataProvider> _customReportDataProviders = new List<ICustomReportDataProvider>();

        public ExceptionProcessor(Context context, ReportField[] reportFields)
        {
            _context = context;
            _reportFields = reportFields;
            _appStartDate = new Time();
            _appStartDate.SetToNow();
            _interactionMode = CrashManager.Config.Mode;
            _initialConfiguration = ReportUtils.GetCrashConfiguration(_context);
            AndroidEnvironment.UnhandledExceptionRaiser += AndroidEnvironmentUnhandledExceptionRaiser;
        }

        private void AndroidEnvironmentUnhandledExceptionRaiser(object sender, RaiseThrowableEventArgs e)
        {
            ProcessException(Throwable.FromException(e.Exception));
        }

        private static readonly object _customReportDataProvidersLocker = new object();
        /// <summary>
        /// Adding custom report data provider.
        /// </summary>
        /// <param name="sender">Custom report data provider instance.</param>
        public void AddCustomReportDataProvider(ICustomReportDataProvider sender)
        {
            lock (_customReportDataProvidersLocker)
            {
                if (_customReportDataProviders.Contains(sender)) return;
                _customReportDataProviders.Add(sender);
            }
        }

        private static readonly object _reportersListLocker = new object();
        /// <summary>
        /// Adding report sender. Thread safe.
        /// </summary>
        /// <param name="sender">Report sender instance.</param>
        public void AddReportSender(IReportSender sender)
        {
            lock (_reportersListLocker)
            {
                if (_reportSenders.Contains(sender)) return;
                _reportSenders.Add(sender);
            }
        }

        /// <summary>
        /// Removing report sender. Thread safe.
        /// </summary>
        /// <param name="sender">Report sender instance.</param>
        public void RemoveReportSender(IReportSender sender)
        {
            lock (_reportersListLocker)
            {
                if (!_reportSenders.Contains(sender)) return;
                _reportSenders.Remove(sender);
            }
        }

        /// <summary>
        /// Build and send report for Exception
        /// </summary>
        /// <param name="th">Throwable that caused exception</param>
        public void ProcessException(Throwable th)
        {
            Log.Error(Constants.LOG_TAG, "Caught a " + th.GetType().Name + " exception for " + _context.PackageName + ". Start building report.");

            var data = ReportDataFactory.BuildReportData(_context, _reportFields, _appStartDate,
                                                         _initialConfiguration, th,
                                                         _interactionMode == InteractionMode.Silent);

            Parallel.ForEach(_customReportDataProviders, s =>
                                                            {
                                                                try
                                                                {
                                                                    var cdata = s.GetReportData(_context);
                                                                    foreach (var d in cdata)
                                                                    {
                                                                        data.Add(d);
                                                                    }
                                                                }
                                                                catch (Exception e)
                                                                {
                                                                    Log.Error(Constants.LOG_TAG, Throwable.FromException(e), "Error getting custom data from " + s.GetType().Name);
                                                                }
                                                            });

            Log.Debug(Constants.LOG_TAG, "Start sending report");
            Parallel.ForEach(_reportSenders, s =>
                                                 {
                                                     try
                                                     {
                                                         Log.Debug(Constants.LOG_TAG, "Start sending report by " + s.GetType().Name);
                                                         s.Send(data);
                                                         Log.Debug(Constants.LOG_TAG, "Report was successfully sent by " + s.GetType().Name);
                                                     }
                                                     catch (ReportSenderException e)
                                                     {
                                                         Log.Error(Constants.LOG_TAG, Throwable.FromException(e), e.Message);
                                                     }
                                                     catch (Exception e)
                                                     {
                                                         Log.Error(Constants.LOG_TAG, Throwable.FromException(e),
                                                                   "Unhandled error when sending report with " +
                                                                   s.GetType().FullName);
                                                     }
                                                 });
            Log.Debug(Constants.LOG_TAG, "Report was builded and sent");
        }

        public void Dispose()
        {
            AndroidEnvironment.UnhandledExceptionRaiser -= AndroidEnvironmentUnhandledExceptionRaiser;
        }
    }
}