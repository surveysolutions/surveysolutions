using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Implementation.Services;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireVersionerTests
{
    internal class QuestionnaireVersionerTestContext
    {
        protected static QuestionnaireVersioner CreateQuestionnaireVersioner()
        {
            return new QuestionnaireVersioner();
        }

        protected static QuestionnaireDocument CreateQuestionnaireDocument()
        {
            return new QuestionnaireDocument
            {
                Children = new List<IComposite>
                {
                    new TextQuestion() {QuestionType = QuestionType.Text}
                }
            }; 
        }

        protected static QuestionnaireDocument CreateQuestionnaireDocumentWithOneChapter(params IComposite[] items)
        {
            return new QuestionnaireDocument
            {
                Children = new List<IComposite>
                {
                    new Group("Chapter")
                    {
                        PublicKey = Guid.Parse("11111111111111111111111111111111"),
                        Children = items.ToList()
                    }
                }
            };
        }

        protected static QuestionnaireDocument CreateQuestionnaireDocumentWithQRQuestion()
        {
            return new QuestionnaireDocument
            {
                Children = new List<IComposite>
                {
                    new QRBarcodeQuestion(){ QuestionType = QuestionType.QRBarcode },
                    new TextQuestion() {QuestionType = QuestionType.Text}
                }
            };
        }

        protected static QuestionnaireDocument CreateQuestionnaireDocumentWithNestedRosters()
        {
            return new QuestionnaireDocument
            {
                Children = new List<IComposite>
                {
                    new Group
                    {
                        IsRoster = true,
                        Children = new List<IComposite>
                        {
                            new QRBarcodeQuestion() { QuestionType = QuestionType.QRBarcode },
                            new Group { IsRoster = true }
                        }
                    }

                }
            };
        }
    }
}
