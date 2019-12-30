using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport
{
    public static class AssignmentsImportExtension
    {
        public static void UseAssignmentsImport(IServiceCollection services, IConfiguration configuration, string section = "AssignmentsImport")
        {
            services.Configure<AssignmentImportOptions>(configuration.GetSection(section));
        }
    }
}
