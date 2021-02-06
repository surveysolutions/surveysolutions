using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Xml.Linq;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Dapper;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace WB.Infrastructure.AspNetCore.DataProtection
{
    public class PostgresXmlRepository : IXmlRepository
    {
        private readonly string connectionString;
        private readonly string schema;

        public PostgresXmlRepository(string connectionString, string schema)
        {
            var npgBuilder = new NpgsqlConnectionStringBuilder(connectionString);
            npgBuilder.SearchPath = schema;
            this.connectionString = npgBuilder.ConnectionString;
            this.schema = schema;
        }

        bool tableChecked = false;
        private bool hasError = false;
        private bool errorResolved = false;
        public static ILogger<PostgresXmlRepository>? Logger { get; set; }

        private void EnsureTableCreated()
        {
            if (tableChecked) return;

            using var db = new NpgsqlConnection(connectionString);
            db.Execute($"create schema if not exists {schema}");
            db.Execute($@"CREATE TABLE if not exists data_protection (
                id uuid NOT NULL,
                value text NOT NULL,
                CONSTRAINT data_protection_pk PRIMARY KEY(id));");
            tableChecked = true;
        }

        public IReadOnlyCollection<XElement> GetAllElements()
        {
            try
            {
                EnsureTableCreated();
                using var db = new NpgsqlConnection(connectionString);
                
                var result = db.Query<string>($"select value from data_protection")
                    .Select(x => XElement.Parse(x))
                    .ToList();

                if (hasError && errorResolved == false)
                {
                    Logger.LogWarning("Postgres data protection xml repository functionality restored");
                    errorResolved = true;
                }
                return result;
            }
            catch (PostgresException pe)
            {
                hasError = true;
                Logger.LogWarning("Cannot read postgres data protection xml repository. Database not exists");
                return new List<XElement>();
            }
        }

        public void StoreElement(XElement element, string friendlyName)
        {
            try
            {
                EnsureTableCreated();
                using var db = new NpgsqlConnection(connectionString);
                db.Execute("INSERT INTO data_protection (id, value) VALUES(@id, @value)", new
                {
                    id = Guid.NewGuid(),
                    value = element.ToString(SaveOptions.DisableFormatting)
                });

                if (hasError && errorResolved == false)
                {
                    Serilog.Log.Warning("Postgres data protection xml repository functionality restored");
                    errorResolved = true;
                }
            }
            catch (PostgresException pe) 
            {
                hasError = true;
                Serilog.Log.Warning("Cannot store postgres data protection xml repository. Database not exists");
            }
        }
    }
}
