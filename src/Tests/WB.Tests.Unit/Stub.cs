using Moq;

namespace WB.Tests.Unit
{
    internal static class Stub<T>
        where T : class
    {
        public static T WithNotEmptyValues
        {
            get { return new Mock<T> { DefaultValue = DefaultValue.Mock }.Object; }
        }
    }
}