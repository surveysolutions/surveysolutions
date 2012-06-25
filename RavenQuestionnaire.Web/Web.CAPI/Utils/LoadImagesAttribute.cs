using System.Linq;
using System.Web.Mvc;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.Views.File;

namespace Web.CAPI.Utils
{
    public class LoadImagesAttribute : ActionFilterAttribute
    {
        private readonly IViewRepository _viewRepository;
        public LoadImagesAttribute(IViewRepository viewRepository)
        {
            _viewRepository = viewRepository;
        }
        private void LoadImages()
        {
            var images = _viewRepository.Load<FileBrowseInputModel, FileBrowseView>(new FileBrowseInputModel { PageSize = int.MaxValue });
            var imagesList = new SelectList(images.Items.Select(i => new SelectListItem
            {
                Selected = false,
                Text = i.Id,
                Value = i.Id
            }).ToList(), "Value", "Text");
        }
    }

}