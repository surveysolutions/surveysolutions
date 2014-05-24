using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.SharedKernels.SurveyManagement.Views.PreloadedData
{
    public class PreloadedContentMetaData
    {
        public PreloadedContentMetaData(string id, string title, PreloadedFileMetaData[] filesMetaInformation, PreloadedContentType preloadedContentType)
        {
            this.PreloadedContentType = preloadedContentType;
            this.Id = id;
            this.Title = title;
            this.FilesMetaInformation = filesMetaInformation;
        }

        public string Id { get; private set; }
        public string Title { get; private set; }
        public PreloadedFileMetaData[] FilesMetaInformation { get; private set; }
        public PreloadedContentType PreloadedContentType { get; private set; }
    }

    public enum PreloadedContentType
    {
        Sample,
        Panel
    }
}
