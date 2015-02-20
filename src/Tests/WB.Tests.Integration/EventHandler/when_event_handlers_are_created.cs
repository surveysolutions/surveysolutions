using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Designer.Implementation.Factories;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo;
using WB.Core.BoundedContexts.Supervisor.Factories;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.EventBus;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.Synchronization.MetaInfo;

using It = Machine.Specifications.It;

namespace WB.Tests.Integration.EventHandler
{
    [Subject(typeof(IEventHandler))]
    internal class when_event_handlers_are_created
    {
        Establish context = () =>
        {

            var type = typeof(IEventHandler);
            var types = new List<Type>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                try
                {
                    types.AddRange(assembly.GetTypes().Where(eh => type.IsAssignableFrom(eh) && !eh.IsAbstract && !eh.IsInterface));
                }
                catch (ReflectionTypeLoadException)
                {
                }
            }

            foreach (var eventHandlerType in types)
            {
                var constructor = eventHandlerType.GetConstructors(BindingFlags.Public | BindingFlags.Instance).First();
                var parameters = constructor.GetParameters();
                var constructorArguments = CreateConstructorArguments(parameters);

                var eventHandlerinstance = constructor.Invoke(constructorArguments) as IEventHandler;

                eventHandlers.Add(eventHandlerType, new EventHandlerDescriptor(eventHandlerType, constructor, eventHandlerinstance));
            }
        };
       
        Because of = () =>
        {
            foreach (var eventHandlerDescriptor in eventHandlers.Values)
            {
                var possibleReadSideAccessors = ExcludeExpectedParameters(eventHandlerDescriptor.Constructor.GetParameters()).ToArray();

                if (possibleReadSideAccessors.Length != eventHandlerDescriptor.Instance.Writers.Length + eventHandlerDescriptor.Instance.Readers.Length)
                    eventHandlersWhereWritersAndReadersCountNotEqualToCountofConstructorArguments.Add(eventHandlerDescriptor.Type);

                foreach (var possibleReadSideAccessor in possibleReadSideAccessors)
                {
                    if(eventHandlerDescriptor.Instance.Writers.Any(w => possibleReadSideAccessor.ParameterType.IsInstanceOfType(w)))
                       continue;

                    if (eventHandlerDescriptor.Instance.Readers.Any(w => possibleReadSideAccessor.ParameterType.IsInstanceOfType(w)))
                        continue;

                    eventHandlersWhereAcessorWasntInjectedViaConstructor.Add(eventHandlerDescriptor.Type);
                }
            }
        };
        // Please check EventHandler's Readers and Writers implementation. It could be possible that not all database accessors are listed in eventhandler's Writers or Readers properties.
        It should_create_event_handlers_where_sum_of_readers_and_writers_is_equal_count_of_constructor_arguments = () =>
            eventHandlersWhereWritersAndReadersCountNotEqualToCountofConstructorArguments.ShouldBeEmpty();

        It should_register_all_respository_accessor_through_consructor = () =>
            eventHandlersWhereAcessorWasntInjectedViaConstructor.ShouldBeEmpty();

        private static Dictionary<Type, EventHandlerDescriptor> eventHandlers = new Dictionary<Type, EventHandlerDescriptor>();
        private static List<Type> eventHandlersWhereWritersAndReadersCountNotEqualToCountofConstructorArguments = new List<Type>();
        private static List<Type> eventHandlersWhereAcessorWasntInjectedViaConstructor = new List<Type>();

        private static Type[] typesToExclude =
        {
            typeof (ILogger), typeof (IQuestionnaireEntityFactory), typeof (IQuestionnaireCacheInitializer),
            typeof (IPlainQuestionnaireRepository), typeof (IQuestionnaireAssemblyFileAccessor), typeof (IExportViewFactory),
            typeof (IQuestionnaireRosterStructureFactory), typeof (IReferenceInfoForLinkedQuestionsFactory),
            typeof (IQuestionDetailsViewMapper), typeof(IJsonUtils), typeof(IMetaInfoBuilder),
            typeof(IInterviewSynchronizationDtoFactory)
        };
      
        private static IEnumerable<ParameterInfo> ExcludeExpectedParameters(ParameterInfo[] allParameters)
        {
            foreach (var parameterInfo in allParameters)
            {
                if(typesToExclude.Contains(parameterInfo.ParameterType))
                    continue;

                yield return parameterInfo;
            }
        }

        private static object[] CreateConstructorArguments(ParameterInfo[] parameters)
        {
            var result = new List<object>();
            
            foreach (var parameterInfo in parameters)
            {
                if (parameterInfo.ParameterType.IsInterface)
                {
                    var instanceOfParameter = typeof (Mock).GetMethod("Of",new Type[0])
                        .MakeGenericMethod(parameterInfo.ParameterType)
                        .Invoke(null, new object[0]);
                    result.Add(instanceOfParameter);
                }
                else
                {
                    result.Add(Activator.CreateInstance(parameterInfo.ParameterType));
                }
            }
            
            return result.ToArray();
        }

        internal class EventHandlerDescriptor
        {
            public EventHandlerDescriptor(Type type, ConstructorInfo constructor, IEventHandler instance)
            {
                this.Type = type;
                this.Constructor = constructor;
                this.Instance = instance;
            }

            public Type Type { get; private set; }
            public ConstructorInfo Constructor { get; private set; }
            public IEventHandler Instance { get; private set; }
        }
    }
}
