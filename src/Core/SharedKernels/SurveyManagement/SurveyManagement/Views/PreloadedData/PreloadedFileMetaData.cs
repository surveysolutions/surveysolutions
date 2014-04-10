using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.SharedKernels.SurveyManagement.Views.PreloadedData
{
    public class PreloadedFileMetaData
    {
        public PreloadedFileMetaData(string fileName, long fileSize)
        {
            this.FileName = fileName;
            this.FileSize = fileSize;
        }

        public string FileName { get; private set; }
        public long FileSize { get; private set; }
    }
}
