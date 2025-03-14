﻿using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Exceptional;
using StackExchange.Exceptional.Stores;

namespace WB.UI.Shared.Web.Exceptions
{
    public static class ExceptionalIntegration
    {
        public static void AddDatabaseStoredExceptional(
            this IServiceCollection services,
            IWebHostEnvironment hostEnvironment,
            IConfiguration configuration)
        {
            
            // this code is needed to run lazy load KnownStoreTypes property
            if (!ErrorStore.KnownStoreTypes.Contains(typeof(PostgreSqlErrorStore)))
                ErrorStore.KnownStoreTypes.Add(typeof(PostgreSqlErrorStore));

            services.AddExceptional(configuration.GetSection("Exceptional"), config =>
            {
                config.UseExceptionalPageOnThrow = hostEnvironment.IsDevelopment();

                config.LogFilters.Header.Add("Authorization", "***");
                config.LogFilters.Form.Add("Password", "***");
                config.LogFilters.Form.Add("ConfirmPassword", "***");
                
                config.LogFilters.Form.Add("__RequestVerificationToken", "***");
                config.LogFilters.Form.Add("Input.Password", "***");
                config.LogFilters.Form.Add("Input.ConfirmPassword", "***");
                config.LogFilters.Form.Add("Input.OldPassword", "***");
                config.LogFilters.Form.Add("Input.NewPassword", "***");
                
                if (config.Store.Type == "PostgreSql")
                {
                    config.Store.TableName = "\"logs\".\"Errors\"";
                    config.Store.ConnectionString = configuration.GetConnectionString("DefaultConnection");
                }
                
                config.OnBeforeLog += (sender, args) =>
                {
                    var ex = args.Error;
                    if (ex?.CustomData != null)
                    {
                        foreach (var key in ex.CustomData.Keys.ToList())
                        {
                            ex.CustomData[key] = ex.CustomData[key]?.Replace("\u0000", "");
                        }
                    }

                    if (!string.IsNullOrEmpty(ex?.Message))
                    {
                        ex.Message = ex.Message.Replace("\u0000", "");
                    }

                    if (!string.IsNullOrEmpty(ex?.Detail))
                    {
                        ex.Detail = ex.Detail.Replace("\u0000", "");
                    }
                };
            });
        }
    }
}
