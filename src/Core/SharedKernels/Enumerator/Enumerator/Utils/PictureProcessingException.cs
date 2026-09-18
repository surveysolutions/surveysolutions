using System;

namespace WB.Core.SharedKernels.Enumerator.Utils
{
    public class PictureProcessingException : Exception
    {
        public PictureProcessingException(string message) : base(message)
        {
        }

        public PictureProcessingException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
