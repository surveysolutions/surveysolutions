using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNet.SignalR.Hubs;
using Prometheus;
using WB.Core.GenericSubdomains.Portable.CustomCollections;
using WB.UI.Headquarters.API.WebInterview.Services;

namespace WB.UI.Headquarters.API.WebInterview.Pipeline
{
    [Localizable(false)]
    public class WebInterviewConnectionsCounter : HubPipelineModule
    {
        public WebInterviewConnectionsCounter()
        {
            this.currentConnectionsCount.Set(0);
        }

        private readonly Gauge currentConnectionsCount = Metrics.CreateGauge(ConnectionLimiter.ConnectedMetricName, @"Number of connection to interview");
        private readonly Gauge messageProcessTime = Metrics.CreateGauge("webinterview_message_processing_time", "Processing time per metric", "type");
        private readonly Counter messagesTotal = Metrics.CreateCounter(@"webinterview_messages_processed", @"Total count messages", "direction");

        private readonly ConcurrentDictionary<string, ConcurrentHashSet<string>> connectedClients = new ConcurrentDictionary<string, ConcurrentHashSet<string>>();

        private ConcurrentHashSet<string> ConnectedToInterview(string interviewId) => this.connectedClients.GetOrAdd(interviewId, key => new ConcurrentHashSet<string>());
        private string InterviewId(IHub hub) => hub.Context.QueryString["interviewId"];

        protected override bool OnBeforeConnect(IHub hub)
        {
            var interviewId = InterviewId(hub);

            var list = ConnectedToInterview(interviewId);
            list.Add(hub.Context.ConnectionId);
            CalculateConnected();
            return base.OnBeforeConnect(hub);
        }

        protected override void OnAfterReconnect(IHub hub)
        {
            var interviewId = InterviewId(hub);
            var list = ConnectedToInterview(interviewId);
            list.Add(hub.Context.ConnectionId);
            CalculateConnected();
            base.OnAfterReconnect(hub);
        }

        protected override void OnAfterDisconnect(IHub hub, bool stopCalled)
        {
            var interviewId = InterviewId(hub);
            var list = ConnectedToInterview(interviewId);
            list.Remove(hub.Context.ConnectionId);
            if (!list.Any())
            {
                ConcurrentHashSet<string> removedList;
                this.connectedClients.TryRemove(interviewId, out removedList);
            }
            CalculateConnected();
            base.OnAfterDisconnect(hub, stopCalled);
        }

        private void CalculateConnected()
        {
            var count = this.connectedClients.Count(l => l.Value.Any());
            
            this.currentConnectionsCount.Set(count);
        }

        private readonly ConcurrentDictionary<string, Stopwatch> _timeMetric = new ConcurrentDictionary<string, Stopwatch>();

        protected override bool OnBeforeIncoming(IHubIncomingInvokerContext context)
        {
            var connId = context.Hub.Context.ConnectionId;
            var key = $"{connId}|{Guid.NewGuid().ToString()}";
            context.Hub.Context.Request.Environment["server.reques.Id"] = key;
            
            this.messagesTotal.Labels("incoming").Inc();

            this._timeMetric.TryAdd(key, Stopwatch.StartNew());

            return base.OnBeforeIncoming(context);
        }

        protected override object OnAfterIncoming(object result, IHubIncomingInvokerContext context)
        {
            var requestIdentifier = context.Hub.Context.Request.Environment["server.reques.Id"] as string;

            if (requestIdentifier != null)
            {
                Stopwatch sw;
                if (this._timeMetric.TryRemove(requestIdentifier, out sw))
                {
                    this.messageProcessTime.Labels("incoming").Set(sw.Elapsed.TotalSeconds);
                }
            }

            return base.OnAfterIncoming(result, context);
        }

        protected override void OnAfterOutgoing(IHubOutgoingInvokerContext context)
        {
            this.messagesTotal.Labels("outgoing").Inc();
        }
    }
}