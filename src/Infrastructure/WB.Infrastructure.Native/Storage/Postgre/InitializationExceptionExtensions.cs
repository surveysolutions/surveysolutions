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

        // Replacement string that masks the password value; not a real credential. // NOSONAR
        private const string PasswordMask = "Password=*****;"; // NOSONAR
        
        public static InitializationException AsInitializationException(this Exception e, string connectionString)
        {
            InitializationException NewDbException(string? message, bool isTransient = false)
            {
                return new InitializationException(Subsystem.Database, message, e)
                {
                    IsTransient = isTransient,
                    Data =
                    {
                        ["ConnectionString"] = ConnectionStringPasswordRegex.Replace(connectionString, PasswordMask)
                    }
                };
            }
            
            if (e is InitializationException ie && ie.Subsystem == Subsystem.Database)
            {
                return new InitializationException(Subsystem.Database, ie.Message, ie)
                {
                    IsTransient = ie.IsTransient,
                    Data =
                    {
                        ["ConnectionString"] = ConnectionStringPasswordRegex.Replace(connectionString, PasswordMask)
                    }
                };
            }

            if (e is PostgresException pe)
            {
                // assume all other errors as transient
                return NewDbException(pe.Message, true);
            }

            if (e is NpgsqlException)
            {
                return NewDbException(Modules.ErrorConnectingToDatabase, true);
            }

            return new InitializationException(Subsystem.Unknown, null, e);
        }
    }
}
