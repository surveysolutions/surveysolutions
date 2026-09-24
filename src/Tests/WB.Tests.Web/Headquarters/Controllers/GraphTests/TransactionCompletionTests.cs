using System;
using System.Data;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using HotChocolate.AspNetCore;
using HotChocolate.Execution;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NHibernate;
using NUnit.Framework;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Workspaces;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql;
using ISession = NHibernate.ISession;

namespace WB.Tests.Web.Headquarters.Controllers.GraphTests
{
    [TestFixture]
    public class TransactionCompletionTests
    {
        private IContainer container;
        private ILifetimeScope lifetimeScope;
        private ServiceProvider services;
        private IServiceScope requestScope;
        private IRequestExecutor executor;
        private UnitOfWork unitOfWork;
        private Mock<ISession> session;
        private Mock<ITransaction> transaction;
        private int fieldsExecuted;

        [SetUp]
        public async Task SetUp()
        {
            fieldsExecuted = 0;
            transaction = CreateTransaction();
            session = new Mock<ISession>();
            session.SetupSequence(x => x.BeginTransaction(IsolationLevel.ReadCommitted))
                .Returns(transaction.Object).Returns(CreateTransaction().Object);
            session.Setup(x => x.CreateSQLQuery("SET TRANSACTION READ ONLY"))
                .Returns(Mock.Of<ISQLQuery>());
            var sessionFactory = new Mock<ISessionFactory>();
            sessionFactory.Setup(x => x.OpenSession()).Returns(session.Object);
            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterInstance(sessionFactory.Object).As<ISessionFactory>();
            container = containerBuilder.Build();
            lifetimeScope = container.BeginLifetimeScope();
            unitOfWork = new UnitOfWork(NullLogger<UnitOfWork>.Instance,
                Mock.Of<IWorkspaceContextAccessor>(), lifetimeScope);

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddScoped<IUnitOfWork>(_ => unitOfWork);
            serviceCollection.AddGraphQLServer()
                .AddTransactionScopeHandler<UnitOfWorkTransactionScopeHandler>()
                .ModifyOptions(o => o.DefaultMutationDependencyInjectionScope = DependencyInjectionScope.Request)
                .ModifyRequestOptions(o => o.IncludeExceptionDetails = false)
                .AddQueryType(x => x.Name("Query").Field("value").Resolve(1))
                .AddMutationType(x =>
                {
                    x.Name("Mutation");
                    x.Field("change").Type<IntType>().Use<TransactionMiddleware>().Resolve(Change);
                    x.Field("reportError").Type<IntType>().Use<TransactionMiddleware>().Resolve(context =>
                    {
                        context.ReportError("Mutation failed");
                        return 0;
                    });
                    x.Field("throwError").Type<IntType>().Use<TransactionMiddleware>()
                        .Resolve(_ => ThrowError());
                });
            services = serviceCollection.BuildServiceProvider();
            requestScope = services.CreateScope();
            executor = await services.GetRequiredService<IRequestExecutorResolver>().GetRequestExecutorAsync();
        }

        [TearDown]
        public async Task TearDown()
        {
            requestScope.Dispose();
            unitOfWork.Dispose();
            await services.DisposeAsync();
            lifetimeScope.Dispose();
            container.Dispose();
        }

        [Test]
        public async Task should_complete_once_after_all_mutation_fields_before_formatting()
        {
            await using var result = await Execute("mutation { first: change second: change }");

            Assert.That(result.ExpectOperationResult().Errors, Is.Null.Or.Empty);
            Assert.That(fieldsExecuted, Is.EqualTo(2));
            transaction.Verify(x => x.Commit(), Times.Once);
            session.Verify(x => x.Dispose(), Times.Never);

            var response = await Format(result);
            Assert.That(response.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            using var json = JsonDocument.Parse(response.Body);
            Assert.That(json.RootElement.GetProperty("data").GetProperty("second").GetInt32(), Is.EqualTo(2));

            requestScope.Dispose();
            transaction.Verify(x => x.Commit(), Times.Once);
            session.Verify(x => x.Dispose(), Times.Once);
        }

        [Test]
        public async Task should_return_commit_failure_before_response_and_not_retry_on_disposal()
        {
            var failure = new InvalidOperationException("Commit failed");
            transaction.Setup(x => x.Commit()).Throws(failure);

            await using var result = await Execute("mutation { first: change second: change }");

            var operation = result.ExpectOperationResult();
            Assert.That(fieldsExecuted, Is.EqualTo(2));
            Assert.That(operation.Data, Is.Null);
            Assert.That(operation.Errors, Has.Count.EqualTo(1));
            Assert.That(operation.Errors[0].Exception, Is.SameAs(failure));
            transaction.Verify(x => x.Commit(), Times.Once);
            session.Verify(x => x.Dispose(), Times.Never);

            var response = await Format(result);
            Assert.That(response.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
            using var json = JsonDocument.Parse(response.Body);
            Assert.That(json.RootElement.GetProperty("errors").GetArrayLength(), Is.EqualTo(1));
            Assert.That(json.RootElement.TryGetProperty("data", out _), Is.False);
            Assert.That(response.Body, Does.Not.Contain(failure.Message));

            requestScope.Dispose();
            transaction.Verify(x => x.Commit(), Times.Once);
            transaction.Verify(x => x.Rollback(), Times.Once);
            session.Verify(x => x.Dispose(), Times.Once);
        }

        [TestCase("reportError")]
        [TestCase("throwError")]
        public async Task should_roll_back_all_fields_when_a_mutation_has_errors(string failingField)
        {
            await using var result = await Execute($"mutation {{ first: change {failingField} last: change }}");

            Assert.That(result.ExpectOperationResult().Errors, Is.Not.Empty);
            Assert.That(fieldsExecuted, Is.EqualTo(2));
            transaction.Verify(x => x.Commit(), Times.Never);
            transaction.Verify(x => x.Rollback(), Times.Once);

            requestScope.Dispose();
            transaction.Verify(x => x.Commit(), Times.Never);
            transaction.Verify(x => x.Rollback(), Times.Once);
        }

        [TestCase("{ value }")]
        [TestCase("mutation { missingField }")]
        public async Task should_not_complete_queries_or_invalid_mutations(string document)
        {
            await using var result = await Execute(document);

            Assert.That(fieldsExecuted, Is.Zero);
            // AcceptChanges rejects an already completed unit of work.
            Assert.DoesNotThrow(() => unitOfWork.AcceptChanges());
            session.Verify(x => x.BeginTransaction(It.IsAny<IsolationLevel>()), Times.Never);
        }

        [Test]
        public void should_discard_accepted_changes_when_execution_exits_without_completing()
        {
            var context = new Mock<IRequestContext>();
            context.SetupGet(x => x.Services).Returns(requestScope.ServiceProvider);
            using (new UnitOfWorkTransactionScopeHandler().Create(context.Object))
            {
                _ = unitOfWork.Session;
                unitOfWork.AcceptChanges();
            }

            requestScope.Dispose();
            transaction.Verify(x => x.Commit(), Times.Never);
            transaction.Verify(x => x.Rollback(), Times.Once);
        }

        private int Change(IResolverContext context)
        {
            Assert.That(context.Service<IUnitOfWork>(), Is.SameAs(unitOfWork));
            transaction.Verify(x => x.Commit(), Times.Never);
            session.VerifySet(x => x.DefaultReadOnly = true, Times.Never);
            _ = unitOfWork.Session;
            return ++fieldsExecuted;
        }

        private static int ThrowError() => throw new InvalidOperationException("Resolver failed");

        private Task<IExecutionResult> Execute(string document) => executor.ExecuteAsync(
            OperationRequestBuilder.New().SetDocument(document).SetServices(requestScope.ServiceProvider).Build());

        private static async Task<(int StatusCode, string Body)> Format(IExecutionResult result)
        {
            var context = new DefaultHttpContext();
            using var body = new MemoryStream();
            context.Response.Body = body;
            await new CompositeFormatter().FormatAsync(context.Response, result,
                Array.Empty<AcceptMediaType>(), null, CancellationToken.None);
            body.Position = 0;
            using var reader = new StreamReader(body);
            return (context.Response.StatusCode, await reader.ReadToEndAsync());
        }

        private static Mock<ITransaction> CreateTransaction()
        {
            var result = new Mock<ITransaction>();
            var active = true;
            result.SetupGet(x => x.IsActive).Returns(() => active);
            result.Setup(x => x.Commit()).Callback(() => active = false);
            result.Setup(x => x.Rollback()).Callback(() => active = false);
            return result;
        }
    }
}

