namespace WB.Core.SharedKernels.ExpressionProcessor.Services
{
    public interface IVariableNameValidator
    {
        string[] GetAllReservedKeywords();
    }
}