using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.SharedKernels.DataCollection.DataTransferObjects.Preloading
{
    public class PreloadedDataDto
    {
        public PreloadedDataDto(string id, PreloadedLevelDto[] data)
        {
            this.Id = id;
            this.Data = data;
        }

        public string Id { get; private set; }
        public PreloadedLevelDto[] Data { get; private set; }
    }
}
