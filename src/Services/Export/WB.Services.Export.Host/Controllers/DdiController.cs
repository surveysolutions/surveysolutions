using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WB.Services.Export.Ddi;
using WB.Services.Infrastructure.Tenant;
using WB.ServicesIntegration.Export;

namespace WB.Services.Export.Host.Controllers
{
    [ApiController]
    public class DdiController : ControllerBase
    {
        private readonly IDdiMetadataAccessor ddiDdiMetadataAccessor;

        public DdiController(IDdiMetadataAccessor ddiDdiMetadataAccessor)
        {
            this.ddiDdiMetadataAccessor = ddiDdiMetadataAccessor ?? throw new ArgumentNullException(nameof(ddiDdiMetadataAccessor));
        }

        [HttpGet]
        [ResponseCache(NoStore = true)]
        [Route("api/v1/ddi")]
        public async Task<FileStreamResult> GetDdiFile(
            string questionnaireId,
            string? archivePassword,
            TenantInfo tenant)
        {
            var pathToFile = await this.ddiDdiMetadataAccessor.GetFilePathToDDIMetadataAsync(tenant,
                new QuestionnaireIdentity(questionnaireId),
                archivePassword);
            var responseStream = System.IO.File.OpenRead(pathToFile);
            return File(responseStream, "application/zip");
        }

    }
}
