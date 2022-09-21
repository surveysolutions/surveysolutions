namespace WB.Core.SharedKernels.Enumerator.Services;

public interface IImageHelper
{
    byte[] GetTransformedArrayOrNull(byte[] value, int maxAllowedDimension);
}
