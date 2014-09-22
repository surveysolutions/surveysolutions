using System;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf;

namespace WB.Tests.Unit
{
    internal static class Create
    {
        public static PdfQuestionnaireView PdfQuestionnaireView(Guid? publicId = null)
        {
            return new PdfQuestionnaireView
            {
                PublicId = publicId ?? Guid.Parse("FEDCBA98765432100123456789ABCDEF"),
            };
        }

        public static PdfQuestionView PdfQuestionView()
        {
            return new PdfQuestionView();
        }

        public static PdfGroupView PdfGroupView()
        {
            return new PdfGroupView();
        }

        public static RoslynExpressionProcessor RoslynExpressionProcessor()
        {
            return new RoslynExpressionProcessor();
        }
    }
}