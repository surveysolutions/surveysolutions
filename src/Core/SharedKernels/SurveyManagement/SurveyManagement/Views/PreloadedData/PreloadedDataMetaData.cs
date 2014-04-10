using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.SharedKernels.SurveyManagement.Views.PreloadedData
{
    public class PreloadedDataMetaData
    {
        public PreloadedDataMetaData(Guid id, string title, PreloadedFileMetaData[] filesMetaInformation)
        {
            this.Id = id;
            this.Title = title;
            this.FilesMetaInformation = filesMetaInformation;
        }

        public Guid Id { get; private set; }
        public string Title { get; private set; }
        public PreloadedFileMetaData[] FilesMetaInformation { get; private set; }
    }
}
