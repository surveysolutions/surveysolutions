using System;
using System.Threading;
using System.Threading.Tasks;
using BinaryData = WB.Services.Export.ExportProcessHandlers.Implementation.BinaryData;

namespace WB.Services.Export.ExportProcessHandlers
{
    internal interface IBinaryDataSource
    {
        Task ForEachInterviewMultimediaAsync(
            ExportState state,
            MultimediaDataType multimediaDataType,
            Func<BinaryData, Task> binaryDataAction, 
            CancellationToken cancellationToken);
    }

    internal enum MultimediaDataType
    {
        Binary = 1,
        AudioAudit = 2
    }
}
