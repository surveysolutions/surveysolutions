using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Dapper;
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
            EnsureTableCreated();
            using var db = new NpgsqlConnection(connectionString);
            return db.Query<string>($"select value from data_protection")
                .Select(x => XElement.Parse(x))
                .ToList();
        }

        public void StoreElement(XElement element, string friendlyName)
        {
            EnsureTableCreated();
            using var db = new NpgsqlConnection(connectionString);
            db.Execute("INSERT INTO data_protection (id, value) VALUES(@id, @value)", new
            {
                id = Guid.NewGuid(),
                value = element.ToString(SaveOptions.DisableFormatting)
            });
        }
    }
}
