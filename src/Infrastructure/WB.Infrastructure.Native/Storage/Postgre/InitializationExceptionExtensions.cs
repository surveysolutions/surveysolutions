#nullable enable
using System;
using System.Text.RegularExpressions;
using Npgsql;
using WB.Core.Infrastructure.Exceptions;
using WB.Core.Infrastructure.Resources;

namespace WB.Infrastructure.Native.Storage.Postgre
{
    public static class InitializationExceptionExtensions
    {
        private static readonly Regex ConnectionStringPasswordRegex =
            new Regex("password=([^;]*);", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        
        public static InitializationException AsInitializationException(this Exception e, string connectionString)
        {
            InitializationException NewDbException(string? message, bool isTransient = false)
            {
                return new InitializationException(Subsystem.Database, message, e)
                {
                    IsTransient = isTransient,
                    Data =
                    {
                        ["ConnectionString"] = ConnectionStringPasswordRegex.Replace(connectionString, "Password=*****;")
                    }
                };
            }
            
            if (e is PostgresException pe)
            {
                return pe.SqlState switch
                {
                    // Invalid authorization
                    "28P01" => NewDbException(Modules.ErrorDatabaseUnauthorized),
                    
                    // Database is starting up
                    "57P03" => NewDbException(null, isTransient: true),
                    _ => NewDbException(null, false)
                };
            }

            if (e is NpgsqlException npe)
            {
                return NewDbException(Modules.ErrorConnectingToDatabase, true);
            }

            return new InitializationException(Subsystem.Unknown, null, e);
        }
    }
}
