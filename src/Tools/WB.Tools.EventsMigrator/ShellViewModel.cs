using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Raven.Abstractions.Extensions;
using WB.Core.GenericSubdomains.Utils;

namespace WB.Tools.EventsMigrator
{
    public class ShellViewModel : Caliburn.Micro.PropertyChangedBase, IShell
    {
        private string serverAddress;
        private string ravenDatabaseName;
        private int processed;
        private int processedPercent;
        private int? totalEvents;
        private string errorMessage;
        private string status;
        private string elapsed;
        private bool canTransfer;
        private ObservableCollection<string> errorMessages;
        private string eventStoreIp;
        private int eventStoreTcpPort;
        private int eventStoreHttpPort;
        private string eventStoreLogin;
        private string eventStorePassword;
        private string appName;
        private ObservableCollection<string> appNames;
        private int skipEvents;

        public ShellViewModel()
        {
            ServerAddress = "http://localhost:8080";
            RavenDatabaseName = "hq-dev";
            EventStoreIP = "127.0.0.1";
            EventStoreHttpPort = 3213;
            EventStoreTcpPort = 3215;
            eventStoreLogin = "admin";
            eventStorePassword = "changeit";
            CanTransfer = true;
            AppNames = new ObservableCollection<string>(new List<string>(){"Designer", "HQ/SV"});
            SelectedAppName = "HQ/SV";
            ErrorMessages = new ObservableCollection<string>();
        }

        public string ServerAddress
        {
            get { return this.serverAddress; }
            set
            {
                this.serverAddress = value;
                this.NotifyOfPropertyChange(() => ServerAddress);
            }
        }

        public string RavenDatabaseName
        {
            get { return this.ravenDatabaseName; }
            set
            {
                this.ravenDatabaseName = value;
                this.NotifyOfPropertyChange(() => RavenDatabaseName);
            }
        }

        public int Processed
        {
            get { return this.processed; }
            set
            {
                this.processed = value;
                this.NotifyOfPropertyChange(() => Processed);
            }
        }

        public int ProcessedPercent
        {
            get { return this.processedPercent; }
            set
            {
                this.processedPercent = value;
                this.NotifyOfPropertyChange(() => ProcessedPercent);
            }
        }

        public int? TotalEvents
        {
            get { return this.totalEvents; }
            set
            {
                this.totalEvents = value;
                this.NotifyOfPropertyChange(() => TotalEvents);
            }
        }

        public string ErrorMessage
        {
            get { return this.errorMessage; }
            set
            {
                this.errorMessage = value;
                this.NotifyOfPropertyChange(() => ErrorMessage);
            }
        }

        public string Status
        {
            get { return this.status; }
            set
            {
                this.status = value;
                this.NotifyOfPropertyChange(() => Status);
            }
        }

        public string Elapsed
        {
            get { return this.elapsed; }
            set
            {
                this.elapsed = value;
                this.NotifyOfPropertyChange(() => Elapsed);
            }
        }

        public bool CanTransfer
        {
            get { return this.canTransfer; }
            set
            {
                this.canTransfer = value;
                this.NotifyOfPropertyChange(() => CanTransfer);
            }
        }

        public ObservableCollection<string> ErrorMessages
        {
            get { return this.errorMessages; }
            set
            {
                this.errorMessages = value;
                this.NotifyOfPropertyChange(() => ErrorMessages);
            }
        }

        public string EventStoreIP
        {
            get { return this.eventStoreIp; }
            set
            {
                this.eventStoreIp = value;
                this.NotifyOfPropertyChange(() => EventStoreIP);
            }
        }

        public int EventStoreTcpPort
        {
            get { return this.eventStoreTcpPort; }
            set
            {
                this.eventStoreTcpPort = value;
                this.NotifyOfPropertyChange(() => EventStoreTcpPort);
            }
        }

        public int EventStoreHttpPort
        {
            get { return this.eventStoreHttpPort; }
            set
            {
                this.eventStoreHttpPort = value; 
                this.NotifyOfPropertyChange(() => EventStoreHttpPort);
            }
        }

        public string EventStoreLogin
        {
            get { return this.eventStoreLogin; }
            set
            {
                this.eventStoreLogin = value;
                this.NotifyOfPropertyChange(() => eventStoreLogin);
            }
        }

        public string EventStorePassword
        {
            get { return this.eventStorePassword; }
            set
            {
                this.eventStorePassword = value; 
                this.NotifyOfPropertyChange(() => eventStorePassword);
            }
        }

        public string SelectedAppName
        {
            get { return this.appName; }
            set
            {
                this.appName = value; 
                this.NotifyOfPropertyChange(() => SelectedAppName);
            }
        }

        public ObservableCollection<string> AppNames
        {
            get { return this.appNames; }
            set
            {
                this.appNames = value; 
                this.NotifyOfPropertyChange(() => AppNames);
            }
        }

        public int SkipEvents
        {
            get { return this.skipEvents; }
            set
            {
                this.skipEvents = value; 
                this.NotifyOfPropertyChange(() => SkipEvents);
            }
        }

        public async Task Transfer()
        {
            var executor = new Executor();
            CanTransfer = false;
            var timer = new DispatcherTimer(DispatcherPriority.Normal, Application.Current.Dispatcher);
            EventHandler timerOnTick = delegate
            {
                var status = executor.GetStatus();
                this.TotalEvents = status.Total;
                this.Status = status.Status;
                this.Processed = status.Processed;
                this.Elapsed = status.Elapsed.ToString("g");

                this.ProcessedPercent = (int)((double)this.Processed / this.TotalEvents * 100);
            };
            timer.Tick += timerOnTick;
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Start();


            try
            {
                await Task.Run(() => executor.Process(this));
            }
            catch (AggregateException e)
            {
                this.ErrorMessages.AddRange(e.UnwrapAllInnerExceptions().Select(x => string.Format("Exception: {0}, Message: {1}", x.GetType().Name, x.Message)));
            }
            catch (Exception e)
            {
                this.ErrorMessages.Add(e.Message);
            }

            CanTransfer = true;
            timer.Stop();
            timerOnTick.Invoke(this, EventArgs.Empty);
        }
    }
}