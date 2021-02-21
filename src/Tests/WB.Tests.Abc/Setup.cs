using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using AutoFixture;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Tests.Abc.Storage;
using WB.Tests.Abc.TestFactories;

namespace WB.Tests.Abc
{
    internal static class SetUp
    {
        public static void InstanceToMockedServiceLocator<TInstance>(TInstance instance)
        {
            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<TInstance>())
                .Returns(instance);
            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance(typeof (TInstance)))
                .Returns(instance);
        }

        public static IQuestionnaireStorage QuestionnaireRepositoryWithOneQuestionnaire(Guid questionnaireId, Expression<Func<IQuestionnaire, bool>> questionnaireMoqPredicate)
        {
            var questionnaire = Mock.Of<IQuestionnaire>(questionnaireMoqPredicate);

            return Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(questionnaireId, questionnaire);
        }

        public static IQuestionnaireStorage QuestionnaireRepositoryWithOneQuestionnaire(QuestionnaireDocument questionnaireDocument)
            => SetUp.QuestionnaireRepositoryWithOneQuestionnaire(Create.Entity.PlainQuestionnaire(questionnaireDocument), questionnaireDocument);

        public static IQuestionnaireStorage QuestionnaireRepositoryWithOneQuestionnaire(
            QuestionnaireIdentity questionnaireIdentity, QuestionnaireDocument questionnaireDocument)
            => SetUp.QuestionnaireRepositoryWithOneQuestionnaire(Create.Entity.PlainQuestionnaire(questionnaireDocument));

        public static IQuestionnaireStorage QuestionnaireRepositoryWithOneQuestionnaire(
            QuestionnaireIdentity questionnaireIdentity, Expression<Func<IQuestionnaire, bool>> questionnaireMoqPredicate)
            => SetUp.QuestionnaireRepositoryWithOneQuestionnaire(Mock.Of<IQuestionnaire>(questionnaireMoqPredicate));

        public static IQuestionnaireStorage QuestionnaireRepositoryWithOneQuestionnaire(IQuestionnaire questionnaire, QuestionnaireDocument questionnaireDocument = null)
        {
            var questionnaireMockStorage = new Mock<IQuestionnaireStorage>();
            questionnaire.EnsureQuestionnaireMockSetup();
            
            questionnaireMockStorage.Setup(x => x.GetQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>(), Moq.It.IsAny<string>())).Returns(questionnaire);
            questionnaireMockStorage.Setup(x => x.GetQuestionnaireOrThrow(Moq.It.IsAny<QuestionnaireIdentity>(), Moq.It.IsAny<string>())).Returns(questionnaire);
            questionnaireMockStorage.Setup(x => x.GetQuestionnaireDocument(Moq.It.IsAny<QuestionnaireIdentity>())).Returns(questionnaireDocument);
            questionnaireMockStorage.Setup(x => x.GetQuestionnaireDocument(Moq.It.IsAny<Guid>(), Moq.It.IsAny<long>())).Returns(questionnaireDocument);

            return questionnaireMockStorage.Object;// Stub<IQuestionnaireStorage>.Returning(questionnaire);
        }

        public static void EnsureQuestionnaireMockSetup(this IQuestionnaire questionnaire)
        {
            if (questionnaire is PlainQuestionnaire)
            {
                return;

            }
            var mocked = Mock.Get(questionnaire);
            if (mocked != null)
            {
                mocked.Setup(q => q.ExpressionStorageType).Returns(typeof(DummyInterviewExpressionStorage));
                mocked.Setup(q => q.GetExpressionsPlayOrder()).Returns(new List<Guid>());
            }
        }

        public static IStatefulInterviewRepository StatefulInterviewRepository(IStatefulInterview interview)
        {
            return Mock.Of<IStatefulInterviewRepository>(_
                => _.Get(It.IsAny<string>()) == interview 
                   && _.GetOrThrow(It.IsAny<string>()) == interview);
        }

        public static Interview InterviewForQuestionnaire(IQuestionnaire questionnaire)
        {
            Guid questionnaireId = Guid.NewGuid();
            long questionnaireVersion = 777;

            IQuestionnaireStorage questionnaireRepository = Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(
                questionnaireId: questionnaireId,
                questionnaireVersion: questionnaireVersion,
                questionnaire: questionnaire);

            Interview interview = Create.AggregateRoot.Interview(questionnaireRepository: questionnaireRepository);

            interview.Apply(Create.Event.InterviewCreated(
                questionnaireId: questionnaireId,
                questionnaireVersion: questionnaireVersion));

            return interview;
        }

        public static Interview InterviewForQuestionnaireDocument(QuestionnaireDocument questionnaireDocument)
        {
            return SetUp.InterviewForQuestionnaire(Create.Entity.PlainQuestionnaire(document: questionnaireDocument));
        }

        public static StatefulInterview StatefulInterview(QuestionnaireDocument questionnaireDocument, bool census = true)
        {
            questionnaireDocument.IsUsingExpressionStorage = true;
            var readOnlyQuestionnaireDocument = questionnaireDocument.AsReadOnly();

            var expressionsPlayOrderProvider = Create.Service.ExpressionsPlayOrderProvider();
            questionnaireDocument.ExpressionsPlayOrder = expressionsPlayOrderProvider.GetExpressionsPlayOrder(readOnlyQuestionnaireDocument); 
            questionnaireDocument.DependencyGraph = expressionsPlayOrderProvider.GetDependencyGraph(readOnlyQuestionnaireDocument); 
            questionnaireDocument.ValidationDependencyGraph = expressionsPlayOrderProvider.GetValidationDependencyGraph(readOnlyQuestionnaireDocument); 

            var questionnaireIdentity = Create.Entity.QuestionnaireIdentity();

            var questionnaireRepository = SetUp.QuestionnaireRepositoryWithOneQuestionnaire(questionnaireIdentity, questionnaireDocument);

            return Create.AggregateRoot.StatefulInterview(
                questionnaireId: questionnaireIdentity.QuestionnaireId,
                questionnaireVersion: questionnaireIdentity.Version,
                questionnaireRepository: questionnaireRepository,
                shouldBeInitialized: census);
        }

        public static ISupportedVersionProvider SupportedVersionProvider(int supportedVerstion)
        {
            var versionProvider = new Mock<ISupportedVersionProvider>();
            versionProvider.Setup(x => x.GetSupportedQuestionnaireVersion()).Returns(supportedVerstion);

            return versionProvider.Object;
        }

        public static IStringCompressor StringCompressor_Decompress<TEntity>(TEntity entity) where TEntity: class
        {
            var zipUtilsMock = new Mock<IStringCompressor>();

            zipUtilsMock.Setup(_ => _.DecompressString<TEntity>(Moq.It.IsAny<string>()))
                .Returns(entity);

            return zipUtilsMock.Object;
        }

        public static IInterviewerPrincipal InterviewerPrincipal(string name, string pass)
        {
            var interviewerIdentity = new InterviewerIdentity() {Name = "name", PasswordHash = "pass"};
            return InterviewerPrincipal(interviewerIdentity);
        }

        public static IInterviewerPrincipal InterviewerPrincipal(InterviewerIdentity interviewerIdentity)
        {
            var interviewerPrincipal = new Mock<IInterviewerPrincipal>();
            interviewerPrincipal.Setup(x => x.CurrentUserIdentity).Returns(interviewerIdentity);
            interviewerPrincipal.Setup(x => x.DoesIdentityExist()).Returns(true);
            interviewerPrincipal.Setup(x => x.GetExistingIdentityNameOrNull()).Returns(interviewerIdentity.Name);
            interviewerPrincipal.Setup(x => x.GetInterviewerByName(interviewerIdentity.Name)).Returns(interviewerIdentity);
            
            interviewerPrincipal.As<IPrincipal>().Setup(x => x.CurrentUserIdentity).Returns(interviewerIdentity);
            return interviewerPrincipal.Object;
        }

        public static IPlainStorageAccessor<TEntity> PlainStorageAccessorWithOneEntity<TEntity>(object id, TEntity entity)
            where TEntity : class
            => new TestPlainStorage<TEntity>(new Dictionary<object, TEntity>
            {
                { id, entity },
            });

        public static FilteredOptionsViewModel FilteredOptionsViewModel(IEnumerable<Answer> optionList)
        {
            var options = optionList
                .Select(x => Create.Entity.CategoricalQuestionOption(Convert.ToInt32(x.AnswerCode.Value), x.AnswerText))
                .ToList();

            Mock<FilteredOptionsViewModel> filteredOptionsViewModel = new Mock<FilteredOptionsViewModel>();
            filteredOptionsViewModel.Setup(x => x.GetOptions(It.IsAny<string>(), It.IsAny<int[]>(), It.IsAny<int?>())).Returns<string, int[], int?>((filter, excludedOptions, count)=>options.FindAll(x=>x.Title.Contains(filter)));
            filteredOptionsViewModel.Setup(x => x.Init(It.IsAny<string>(), It.IsAny<Identity>(), It.IsAny<int>()));

            return filteredOptionsViewModel.Object;
        }

        public static FilteredOptionsViewModel FilteredOptionsViewModel(List<CategoricalOption> optionList = null)
        {
            var options = optionList ?? new List<CategoricalOption>
            {
                Create.Entity.CategoricalQuestionOption(1, "abc"),
                Create.Entity.CategoricalQuestionOption(2, "bbc"),
                Create.Entity.CategoricalQuestionOption(3, "bbc"),
                Create.Entity.CategoricalQuestionOption(4, "bbaé"),
                Create.Entity.CategoricalQuestionOption(5, "cccé"),
            };

            Mock<FilteredOptionsViewModel> filteredOptionsViewModel = new Mock<FilteredOptionsViewModel>();
            filteredOptionsViewModel.Setup(x => x.GetOptions(It.IsAny<string>(), It.IsAny<int[]>(), It.IsAny<int?>())).Returns<string, int[], int?>((filter, excludedOptions, count) =>options.FindAll(x=>x.Title.Contains(filter??string.Empty)));
            filteredOptionsViewModel.Setup(x => x.Init(It.IsAny<string>(), It.IsAny<Identity>(), It.IsAny<int>()));
            filteredOptionsViewModel.Setup(x => x.GetAnsweredOption(It.IsAny<int>())).Returns<int>(filter => options.Find(x => x.Value == filter ));
            
            return filteredOptionsViewModel.Object;
        }

        internal static void ApplyInterviewEventsToViewModels(IEventSourcedAggregateRoot interview, IViewModelEventRegistry eventRegistry, Guid interviewId)
        {
            foreach (var evnt in interview.GetUnCommittedChanges().Select(x => Create.Other.CommittedEvent(x, interviewId)))
            {
                foreach (var viewModel in eventRegistry.GetViewModelsByEvent(evnt))
                {
                    var isAsyncHandler = viewModel
                        .GetType()
                        .GetTypeInfo()
                        .ImplementedInterfaces
                        .Any(type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IAsyncViewModelEventHandler<>));

                    var methodName = $"Handle{(isAsyncHandler ? "Async" : "")}";

                    var handler = viewModel.GetType().GetRuntimeMethod(methodName, new[] { evnt.Payload.GetType() });

                    var taskOrVoid = (Task)handler?.Invoke(viewModel, new object[] { evnt.Payload });
                    taskOrVoid?.WaitAndUnwrapException();
                }
            }
        }

        internal static StatefulInterview StatefulInterviewWithMultilanguageQuestionnaires(
             KeyValuePair<string, IComposite[]>[] questionnaires, 
             IQuestionOptionsRepository questionOptionsRepository = null)
        {
            var chapterId = Guid.Parse("33333333333333333333333333333333");

            var questionnaireDocuments = new List<KeyValuePair<string, QuestionnaireDocument>>();

            var langs = questionnaires.Select(x => x.Key).Where(x => x != null).ToArray();

            var translations = new List<Translation>(
                langs.Select(x => Create.Entity.Translation(Guid.NewGuid(), x)));

            var translationIndex = 0;

            foreach (var questionnaire in questionnaires)
            {
                var questionnaireDocumentWithOneChapterAndLanguages =
                    Create.Entity.QuestionnaireDocumentWithOneChapterAndLanguages(
                        chapterId,
                        translations,
                        questionnaire.Key ==null ? null : translations[translationIndex++]?.Id,
                        questionnaire.Value);

                questionnaireDocuments.Add(
                    new KeyValuePair<string, QuestionnaireDocument>(
                        questionnaire.Key, questionnaireDocumentWithOneChapterAndLanguages));
            }

            var questionnaireRepository = Create.Fake.QuestionnaireRepository(
                questionnaireDocuments.ToArray(), translations, questionOptionsRepository);

            return Create.AggregateRoot.StatefulInterview(
                questionnaireRepository: questionnaireRepository, 
                questionOptionsRepository: questionOptionsRepository);
        }

        public static Mock<T> GetMock<T>(this IFixture fixture) where T : class
        {
            return fixture.Freeze<Mock<T>>();
        }
        
        public static void FreezeMock<T>(this IFixture fixture) where T : class
        {
            fixture.Freeze<Mock<T>>();
        }

        public static IPlainStorageAccessor<QuestionnaireBrowseItem> QuestionnaireBrowseItemRepository(params QuestionnaireBrowseItem[] questionnaireBrowseItem)
        {
            var questionnaireBrowseItemStorage = Mock.Of<IPlainStorageAccessor<QuestionnaireBrowseItem>>();

            Mock.Get(questionnaireBrowseItemStorage)
                .Setup(reader => reader.Query(It.IsAny<Func<IQueryable<QuestionnaireBrowseItem>, List<QuestionnaireBrowseItem>>>()))
                .Returns<Func<IQueryable<QuestionnaireBrowseItem>, List<QuestionnaireBrowseItem>>>(query => query.Invoke(questionnaireBrowseItem.AsQueryable()));

            return questionnaireBrowseItemStorage;
        }

        public static IQueryable<TEntity> GetNhQueryable<TEntity>(this IQueryable<TEntity> source)
        {
            return new TestingQueryable<TEntity>(source);
        }
    }
}
