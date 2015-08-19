using System;
using System.Linq;
using Android.App;
using Android.Util;
using Mono.Android.Crasher.Attributes;
using Mono.Android.Crasher.Data;
using Mono.Android.Crasher.Data.Submit;

namespace Mono.Android.Crasher
{
    public static class CrashManager
    {
        /// <summary>
        /// Default report fields that will be used if <see cref="CrasherAttribute.ReportContent"/> not set.
        /// </summary>
        public static ReportField[] DefaultReportFields = {
                                                              ReportField.ReportID, ReportField.AppVersionCode, ReportField.AppVersionName, ReportField.PackageName,
                                                              ReportField.FilePath, ReportField.PhoneModel, ReportField.Brand, ReportField.Product, ReportField.AndroidVersion, ReportField.Build, ReportField.TotalMemSize, ReportField.AvailableMemSize,
                                                              ReportField.IsSilent, ReportField.StackTrace, ReportField.InitialConfiguration, ReportField.CrashConfiguration, ReportField.Display, ReportField.UserComment,
                                                              ReportField.UserAppStartDate, ReportField.UserCrashDate, ReportField.DumpsysMeminfo, ReportField.Logcat, ReportField.Eventslog, ReportField.Radiolog,
                                                              ReportField.DeviceID, ReportField.InstallationID, ReportField.DeviceFeatures, ReportField.Environment, ReportField.SharedPreferences,
                                                              ReportField.SettingsSystem, ReportField.SettingsSecure 
                                                          };

        private static CrasherAttribute _config;
        private static Application _application;

        internal static CrasherAttribute Config
        {
            get
            {
                return _config;
            }
        }

        private static ExceptionProcessor _exceptionProcessor;
        /// <summary>
        /// Instance of <see cref="ExceptionProcessor"/> attached to current application.
        /// </summary>
        internal static ExceptionProcessor ExceptionProcessor
        {
            get { return _exceptionProcessor; }
        }

        /// <summary>
        /// Make basic Crasher initializations to handle exceptions from <see cref="Application"/>
        /// </summary>
        /// <param name="app"><see cref="Application"/> instance to monitor for exceptions.</param>
        public static void Initialize(Application app)
        {
            if (_application != null)
            {
                throw new InvalidOperationException("CrashReporter# init called more than once");
            }

            _application = app;
            _config = app.GetType().GetCustomAttributes(typeof(CrasherAttribute), false).FirstOrDefault() as CrasherAttribute;

            if (_config == null)
            {
                Log.Error(Constants.LOG_TAG, "CrashReporter# init called but no CrasherAttribute on Application class " + _application.PackageName);
                return;
            }

            Log.Debug(Constants.LOG_TAG, "CrasherAttribute is enabled for " + _application.PackageName + ", intializing...");
            _exceptionProcessor = new ExceptionProcessor(_application.ApplicationContext, _config.ReportContent ?? DefaultReportFields);

            if (_config.UseCustomData && _config.CustomDataProviders != null)
            {
                foreach (var customDataProvider in _config.CustomDataProviders)
                {
                    _exceptionProcessor.AddCustomReportDataProvider(Activator.CreateInstance(customDataProvider) as ICustomReportDataProvider);
                }
            }
        }

        /// <summary>
        /// Add sender to senders list.
        /// </summary>
        /// <typeparam name="T">Implementaion of <see cref="IReportSender"/></typeparam>
        /// <param name="valueFactory">Function that constructs new instance of sender</param>
        public static void AttachSender<T>(Func<T> valueFactory) where T : class, IReportSender
        {
            if (_application == null || _exceptionProcessor == null)
                throw new InvalidOperationException("Need to call AttachSender method after Initialize");

            //TODO Wait for new version of MonoDroid to use reflection for instantiation
            //var sender = Activator.CreateInstance(typeof(T)) as IReportSender;
            var sender = valueFactory();
            if (sender == null)
                throw new NullReferenceException("Could not create instance of " + typeof(T));
            sender.Initialize(_application);
            _exceptionProcessor.AddReportSender(sender);
        }

        /// <summary>
        /// Remove sender from senders list.
        /// </summary>
        /// <typeparam name="T">Implementaion of <see cref="IReportSender"/></typeparam>
        /// <param name="reporter">ReportSender to remove from senders list</param>
        public static void DetachSender<T>(T reporter) where T : IReportSender
        {
            if (_application == null || _exceptionProcessor == null)
                throw new InvalidOperationException("Need to call DetachSender method after Initialize");
            _exceptionProcessor.RemoveReportSender(reporter);
        }

        /// <summary>
        /// Process handled exception
        /// </summary>
        /// <param name="exception">Handled exception</param>
        public static void HandleException(Exception exception)
        {
            if (exception != null)
                HandleException(Java.Lang.Throwable.FromException(exception));
        }

        /// <summary>
        /// Process handled exception
        /// </summary>
        /// <param name="tr">Handled <see cref="Java.Lang.Throwable"/> that caused exception</param>
        public static void HandleException(Java.Lang.Throwable tr)
        {
            if (_exceptionProcessor != null)
                _exceptionProcessor.ProcessException(tr);
        }

        /// <summary>
        /// Stop proccessing application exceptions
        /// </summary>
        public static void Stop()
        {
            if (_exceptionProcessor != null)
                _exceptionProcessor.Dispose();
            _exceptionProcessor = null;
            _config = null;
            _application = null;
        }
    }
}