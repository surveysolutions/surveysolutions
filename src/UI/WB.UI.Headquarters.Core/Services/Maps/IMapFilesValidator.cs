using System.Collections.Generic;

namespace WB.UI.Headquarters.Services.Maps;

public interface IMapFilesValidator
{
    IEnumerable<ValidatorError> Verify(AnalyzeResult analyzeResults);
}