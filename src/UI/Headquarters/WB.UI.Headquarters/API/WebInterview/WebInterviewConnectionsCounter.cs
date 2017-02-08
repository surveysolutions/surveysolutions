using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNet.SignalR.Hubs;
using Prometheus;

namespace WB.UI.Headquarters.API.WebInterview
{
    [Localizable(false)]
    public class WebInterviewConnectionsCounter : HubPipelineModule
    {
        public WebInterviewConnectionsCounter()
        {
            this.currentConnectionsCount.Set(0);
        }

        private readonly Gauge currentConnectionsCount = Metrics.CreateGauge(ConnectionLimiter.ConnectedMetricName, @"Number of connection to interview");
        private readonly Gauge messageProcessTime = Metrics.CreateGauge("webinterview_message_processing_time", "Processing time per metric", "type", "action");
        private readonly Counter messagesTotal = Metrics.CreateCounter(@"webinterview_messages_processed", @"Total count messages", "direction", "action");

        private readonly ConcurrentDictionary<string, HashSet<string>> connectedClients = new ConcurrentDictionary<string, HashSet<string>>();

        protected override bool OnBeforeConnect(IHub hub)
        {
            var interviewId = hub.Context.QueryString.Get(@"interviewId");
            this.connectedClients.AddOrUpdate(interviewId,
                id =>
                {
                    this.currentConnectionsCount.Inc();
                    return new HashSet<string>() { hub.Context.ConnectionId };
                }, (id, list) =>
                {
                    if (!list.Any())
                    {
                        this.currentConnectionsCount.Inc();
                    }

                    list.Add(hub.Context.ConnectionId);
                    return list;
                });
            
            return base.OnBeforeConnect(hub);
        }

        protected override void OnAfterReconnect(IHub hub)
        {
            var interviewId = hub.Context.QueryString.Get(@"interviewId");

            this.connectedClients.AddOrUpdate(interviewId,
                id =>
                {
                    this.currentConnectionsCount.Inc();
                    return new HashSet<string> {hub.Context.ConnectionId};
                }, 
                (id, list) =>
                {
                    list.Add(hub.Context.ConnectionId);
                    return list;
                });

            base.OnAfterReconnect(hub);
        }

        protected override void OnAfterDisconnect(IHub hub, bool stopCalled)
        {
            var interviewId = hub.Context.QueryString.Get(@"interviewId");
            this.connectedClients.AddOrUpdate(interviewId, key => new HashSet<string>(),
                (id, list) =>
                {
                    bool isListEmpty = !list.Any();
                    list.Remove(hub.Context.ConnectionId);

                    if (!isListEmpty && !list.Any())
                    {
                        this.currentConnectionsCount.Dec();
                    }
                    return list;
                });
            base.OnAfterDisconnect(hub, stopCalled);
        }

        private readonly ConcurrentDictionary<string, Stopwatch> _timeMetric = new ConcurrentDictionary<string, Stopwatch>();

        protected override bool OnBeforeIncoming(IHubIncomingInvokerContext context)
        {
            var connId = context.Hub.Context.ConnectionId;
            this.messagesTotal.Labels("incoming", "").Inc();

            while (!this._timeMetric.TryAdd(connId, Stopwatch.StartNew())) ;

            return base.OnBeforeIncoming(context);
        }

        protected override object OnAfterIncoming(object result, IHubIncomingInvokerContext context)
        {
            var connId = context.Hub.Context.ConnectionId;
            Stopwatch sw;
            if (this._timeMetric.TryRemove(connId, out sw))
            {
                this.messageProcessTime.Labels("incoming", "").Set(sw.Elapsed.TotalSeconds);
            }

            return base.OnAfterIncoming(result, context);
        }
    }
}