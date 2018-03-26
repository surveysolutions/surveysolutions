using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Verifier;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.PreloadedData;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation.ServiceVariables;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadedDataVerifierTests
{
    [Subject(typeof(ImportDataVerifier))]
    internal class PreloadedDataVerifierTestContext
    {
        protected static ImportDataVerifier CreatePreloadedDataVerifier(
            QuestionnaireDocument questionnaireDocument = null, 
            IPreloadedDataService preloadedDataService = null,
            IUserViewFactory userViewFactory=null)
        {
            return new ImportDataVerifier(userViewFactory ?? Mock.Of<IUserViewFactory>(), TODO);
        }

        protected static PreloadedDataByFile CreatePreloadedDataByFile(string[] header=null, string[][] content=null, string fileName=null)
        {
            return new PreloadedDataByFile(fileName ?? "some file", header ?? new string[] { ServiceColumns.InterviewId, ServiceColumns.ParentId },
                content ?? new string[0][]);
        }

        protected static QuestionnaireDocument CreateQuestionnaireDocumentWithOneChapter(params IComposite[] chapterChildren)
        {
            return new QuestionnaireDocument
            {
                Title = "some title",
                Children = new List<IComposite>
                {
                    new Group("Chapter")
                    {
                        PublicKey = Guid.Parse("FFF000AAA111EE2DD2EE111AAA000FFF"),
                        Children = chapterChildren?.ToReadOnlyCollection() ?? new ReadOnlyCollection<IComposite>(new List<IComposite>()),
                        IsRoster = false
                    }
                }.ToReadOnlyCollection()
            };
        }

        protected static List<PanelImportVerificationError> VerificationErrors = Array.Empty<PanelImportVerificationError>().ToList();
    }
}
