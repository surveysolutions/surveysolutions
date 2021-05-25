using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Newtonsoft;
using Swashbuckle.AspNetCore.SwaggerGen;
using WB.UI.Headquarters.Code.SwaggerCustomization;

namespace WB.UI.Headquarters.Code
{
    public static class SwaggerIntegration
    {
        public static void AddHqSwaggerGen(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Survey Solutions API",
                    Version = "v1"
                });
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);

                c.AddSecurityDefinition("basicAuth", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "basic"
                });
                c.ParameterFilter<XmsEnumParameterFilter>();
                c.OperationFilter<XmsEnumOperationFilter>();
                c.SchemaFilter<XmsEnumSchemaFilter>();
                c.DocumentFilter<WorkspaceDocumentFilter>();

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "basicAuth"
                            }
                        },
                        Array.Empty<string>()
                    }
                });

                c.OrderActionsBy(x =>
                {
                    var sort = new StringBuilder(x.ActionDescriptor.RouteValues["controller"]);
                    sort.Append("_");
                    sort.Append(x.RelativePath.Split('/')[1]);
                    sort.Append("_");
                    if (x.HttpMethod == "GET")
                    {
                        sort.Append("A");
                    }
                    else if (x.HttpMethod == "PATCH")
                    {
                        sort.Append("B");
                    }
                    else if (x.HttpMethod == "POST")
                    {
                        sort.Append("C");
                    }
                    else if (x.HttpMethod == "PUT")
                    {
                        sort.Append("D");
                    }
                    else if (x.HttpMethod == "DELETE")
                    {
                        sort.Append("E");
                    }

                    sort.Append("_");
                    sort.Append(x.ActionDescriptor.AttributeRouteInfo.Template);
                    var result = sort.ToString();
                    return result;
                });
                c.TagActionsBy(x => 
                    new List<string> {x.ActionDescriptor.RouteValues["controller"].Replace("PublicApi", "")});
            });

            services.Replace(
                ServiceDescriptor.Transient<ISerializerDataContractResolver>(s =>
                    new NewtonsoftDataContractResolver(PublicApiJsonAttribute.PublicApiSerializerSettings)));
        }

        public static void UseHqSwaggerUI(this IApplicationBuilder appBuilder)
        {
            appBuilder.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Survey Solutions API");
                c.RoutePrefix = "apidocs";
            });
        }
    }

}
