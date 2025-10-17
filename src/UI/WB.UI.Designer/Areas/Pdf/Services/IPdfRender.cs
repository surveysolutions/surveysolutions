using System.Threading;
using System.Threading.Tasks;
namespace WB.UI.Designer.Areas.Pdf.Services;

public interface IPdfRender
{
    Task<byte[]> RenderPdf(string questionnaireHtml, string footerHtml, CancellationToken token);
}
