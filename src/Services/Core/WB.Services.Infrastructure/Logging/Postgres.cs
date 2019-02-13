using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using Dapper;
using Npgsql;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Json;

namespace WB.Services.Infrastructure.Logging
{
    public class Postgres : ILogEventSink
    {
        private readonly string connectionString;
        private readonly string schema;
        private readonly string tableName;
        private readonly LogEventLevel minLevel;
        private readonly Subject<LogEvent> queue;
        private readonly JsonFormatter formatter = new JsonFormatter();
        private bool tableCreated = false;

        public Postgres(string connectionString, string schema, string tableName, LogEventLevel minLevel)
        {
            this.connectionString = connectionString;
            this.schema = schema;
            this.tableName = tableName;
            this.minLevel = minLevel;

            this.queue = new Subject<LogEvent>();

            queue.GroupByUntil(
                    // yes. yes. all items belong to the same group.
                    x => true,
                    g => Observable.Amb(
                        // close the group after 5 seconds of inactivity
                        g.Throttle(TimeSpan.FromSeconds(1)),
                        // close the group after 10 items
                        g.Skip(10)
                    ))
                // Turn those groups into buffers
                .SelectMany(x => x.ToArray())
                .Subscribe(events =>
                {
                    if (events.Length == 0) return;

                    if (!tableCreated)
                    {
                        EnsureTableCreated();
                        tableCreated = true;
                    }

                    var sb = new StringBuilder();
                    using (var db = new NpgsqlConnection(connectionString))
                    {
                        db.Execute($@"insert into ""{schema}"".""{tableName}"" " 
                                   + "( message, jobId, source, stacktrace, level, tenant, data, timestamp, host, version, app)" 
                                   + " values (@message, @jobId, @source, @stackTrace, @level, @tenant, @data::jsonb, @timestamp, "
                                   + " @host, @version, @app)",
                            events.Select(e =>
                            {
                                string GetProperty(string name) 
                                {
                                    return e.Properties.ContainsKey(name) ? e.Properties[name].ToString() : null;
                                }

                                sb.Clear();
                                var text = new StringWriter(sb);
                                formatter.Format(e, text);
                            
                                var jobId = GetProperty("jobId");
                                var data = sb.ToString();
                            
                                return new
                                {
                                    app = GetProperty("AppType"),
                                    data,
                                    host = GetProperty("Host"),
                                    jobId = jobId != null ? int.Parse(jobId) : (int?)null,
                                    level = e.Level.ToString(),
                                    message = e.RenderMessage(),
                                    source= GetProperty("SourceContext"),
                                    stackTrace = e.Exception?.ToStringDemystified(),
                                    tenant = GetProperty("tenantName"),
                                    timestamp = e.Timestamp,
                                    version = GetProperty("Version")
                                };
                            }));
                    }
                });
        }


        private void EnsureTableCreated()
        {
            using (var db = new NpgsqlConnection(connectionString))
            {
                db.Execute($@"create schema if not exists ""{schema}""  ");
                db.Execute($@"CREATE TABLE if not exists ""{schema}"".""{tableName}"" (
	id serial NOT NULL,
	message varchar NULL,
    level varchar null,
	tenant varchar NULL,
    jobId int null,
    source varchar null,
	""timestamp"" timestamp NULL,
    version varchar null,
    app varchar null,    
    host varchar null,
	""data"" jsonb NULL,
    stacktrace varchar null,
	CONSTRAINT errors_pk PRIMARY KEY (id)
);
CREATE INDEX if not exists {tableName}_tenant_idx ON ""{schema}"".""{tableName}"" (tenant);
CREATE INDEX if not exists  {tableName}_timestamp_idx ON ""{schema}"".""{tableName}"" (""timestamp"");");
            }
        }

        public void Emit(LogEvent logEvent)
        {
            if(logEvent.Level >= minLevel)
                queue.OnNext(logEvent);
        }
    }
}
