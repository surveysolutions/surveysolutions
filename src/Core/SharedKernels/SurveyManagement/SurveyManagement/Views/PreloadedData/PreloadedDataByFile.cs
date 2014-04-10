using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.SharedKernels.SurveyManagement.Views.PreloadedData
{
    public class PreloadedDataByFile
    {
        public PreloadedDataByFile(Guid id, string fileName, string[] header, string[][] content)
        {
            this.Id = id;
            this.FileName = fileName;
            this.Header = header;
            this.Content = content;
        }

        public Guid Id { get; private set; }
        public string FileName { get; private set; }
        public string[] Header { get; private set; }
        public string[][] Content { get; private set; }
    }
}
