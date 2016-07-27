using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace WB.Core.BoundedContexts.Designer.Translations
{
    [Serializable]
    public class InvalidExcelFileException : Exception
    {
        public InvalidExcelFileException()
        {
        }

        public InvalidExcelFileException(string message)
            : base(message)
        {
        }

        public InvalidExcelFileException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected InvalidExcelFileException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }

        public List<TranslationValidationError> FoundErrors { get; set; }
    }
}