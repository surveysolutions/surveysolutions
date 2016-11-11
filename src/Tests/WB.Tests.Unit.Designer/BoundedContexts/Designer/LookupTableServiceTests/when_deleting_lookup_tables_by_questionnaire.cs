using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Designer.Implementation.Services.LookupTableService;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.LookupTableServiceTests
{
    internal class when_deleting_lookup_tables_by_questionnaire
    {
        private Establish context = () =>
        {
            LookupTableContentStorageMock
                .Setup(x => x.Store(Moq.It.IsAny<LookupTableContent>(), Moq.It.IsAny<string>()))
                .Callback((LookupTableContent content, string id) => { lookupTableContent = content; });

            var lookupTables = new Dictionary<Guid, LookupTable>();
            lookupTables.Add(lookupTableId, new LookupTable());


            var questionnaireDocument = new  QuestionnaireDocument() {PublicKey = questionnaireId };
            questionnaireDocument.LookupTables = lookupTables;

            var qStore = new Mock<IPlainKeyValueStorage<QuestionnaireDocument>>();
            qStore.Setup(x=> x.GetById(Moq.It.IsAny<string>()))
                .Returns(questionnaireDocument);
            

            fileContent =
               $"no{_}rowcode{_}column{_end}" +
               $"1{_}2{_}3{_end}" +
               $"2{_}3{_}4{_end}";

            lookupTableService = Create.LookupTableService(lookupTableContentStorage: LookupTableContentStorageMock.Object, documentStorage: qStore.Object);
            lookupTableService.SaveLookupTableContent(questionnaireId, lookupTableId, fileContent);

        };

        Because of = () =>
                lookupTableService.DeleteAllByQuestionnaireId(questionnaireId);

        It should_ = () =>
            LookupTableContentStorageMock.Verify(x => x.Remove(Moq.It.IsAny<string>()),Times.Once);
        
        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid lookupTableId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static string fileContent;
        private static LookupTableService lookupTableService;

        private static LookupTableContent lookupTableContent;
        private static readonly Mock<IPlainKeyValueStorage<LookupTableContent>> LookupTableContentStorageMock = new Mock<IPlainKeyValueStorage<LookupTableContent>>();

        private static string _ = "\t";
        private static string _end = "\n";
    }
}