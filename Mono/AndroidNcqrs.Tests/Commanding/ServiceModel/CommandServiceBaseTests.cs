//todo: Strange behavior in tests...try to fix with Mocks framework replacement
//using System;
//using AndroidMocks;
//using FluentAssertions;
//using Ncqrs.Commanding.ServiceModel;
//using NUnit.Framework;
//using Ncqrs.Commanding.CommandExecution;
//using Ncqrs.Commanding;

//namespace Ncqrs.Tests.Commanding.ServiceModel
//{
//    [TestFixture]
//    public class CommandServiceBaseTests
//    {
//        private DynamicMock<ICommandServiceInterceptor> _interceptor1Mock;
//        private DynamicMock<ICommandServiceInterceptor> _interceptor2Mock;
//        private DynamicMock<ICommandExecutor<CommandWithExecutor>> _executorForCommandWithExecutorMock;

//        public class CommandWithExecutor : CommandBase
//        { }

//        public class CommandWithoutExecutor : CommandBase
//        { }

//        public class CommandWithExecutorThatThrowsException : CommandBase
//        { }

//        private ICommandService TheService
//        {
//            get;
//            set;
//        }

//        private ICommandExecutor<CommandWithExecutor> ExecutorForCommandWithExecutor
//        {
//            get;
//            set;
//        }

//        private ICommandExecutor<CommandWithExecutorThatThrowsException> ExecutorForCommandWithExecutorThatThrowsException
//        { get; set; }

//        private ICommandServiceInterceptor Interceptor1
//        {
//            get;
//            set;
//        }

//        private ICommandServiceInterceptor Interceptor2
//        {
//            get;
//            set;
//        }

//        [SetUp]
//        public void Setup()
//        {
//            var service = new CommandService();
//            _executorForCommandWithExecutorMock = new DynamicMock<ICommandExecutor<CommandWithExecutor>>();
//            ExecutorForCommandWithExecutor = _executorForCommandWithExecutorMock.Instance;

//            var executorForCommandWithExecutorThatThrowsExceptionMock = new DynamicMock<ICommandExecutor<CommandWithExecutorThatThrowsException>>();
//            ExecutorForCommandWithExecutorThatThrowsException = executorForCommandWithExecutorThatThrowsExceptionMock.Instance;

//            _interceptor1Mock = new DynamicMock<ICommandServiceInterceptor>();
//            _interceptor2Mock = new DynamicMock<ICommandServiceInterceptor>();
//            _interceptor1Mock.Expect(i => i.OnBeforeExecution(null));
//            _interceptor2Mock.Expect(i => i.OnBeforeExecution(null));
//            _interceptor1Mock.Expect(i => i.OnAfterExecution(null));
//            _interceptor2Mock.Expect(i => i.OnAfterExecution(null));

//            Interceptor1 = _interceptor1Mock.Instance;
//            Interceptor2 = _interceptor2Mock.Instance;

//            executorForCommandWithExecutorThatThrowsExceptionMock.StubAndThrow<Exception>(e => e.Execute(null));

//            service.RegisterExecutor(ExecutorForCommandWithExecutor);
//            service.RegisterExecutor(ExecutorForCommandWithExecutorThatThrowsException);

//            service.AddInterceptor(Interceptor1);
//            service.AddInterceptor(Interceptor2);

//            TheService = service;
//        }

//        [Test]
//        public void All_interceptors_should_be_called_before_execution()
//        {
//            _interceptor1Mock.Reset();
//            _interceptor2Mock.Reset();

//            TheService.Execute(new CommandWithExecutor());

//            _interceptor1Mock.AssertWasCalled(i => i.OnBeforeExecution(null));
//            _interceptor2Mock.AssertWasCalled(i => i.OnBeforeExecution(null));
//        }

//        [Test]
//        public void All_interceptors_should_be_called_after_execution()
//        {
//            _interceptor1Mock.Reset();
//            _interceptor2Mock.Reset();

//            TheService.Execute(new CommandWithExecutor());

//            _interceptor1Mock.AssertWasCalled(i => i.OnAfterExecution(null));
//            _interceptor2Mock.AssertWasCalled(i => i.OnAfterExecution(null));
//        }

//        [Test]
//        public void Executing_command_with_no_handler_should_cause_exception()
//        {
//            Action act = () => TheService.Execute(new CommandWithoutExecutor());
//            act.ShouldThrow<ExecutorForCommandNotFoundException>();
//        }

//        [Test]
//        public void Executing_command_should_executure_correct_handler_with_it()
//        {
//            var theCommand = new CommandWithExecutor();

//            _executorForCommandWithExecutorMock.Reset();
//            TheService.Execute(theCommand);

//            _executorForCommandWithExecutorMock.AssertWasCalled(e => e.Execute(theCommand));
//        }
//    }
//}
