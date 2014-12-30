using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Machine.Specifications;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewHistoryDenormalizerTests
{
    [Subject(typeof(InterviewHistoryDenormalizer))]
    internal class when_all_available_descendants_of_QuestionAnswered_recived
    {
        Establish context = () =>
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                try
                {
                    availableDescendants.AddRange(
                        assembly.GetTypes().Where(eh => typeof (QuestionAnswered).IsAssignableFrom(eh) && !eh.IsAbstract && !eh.IsInterface));
                }
                catch (ReflectionTypeLoadException)
                {
                }
            }
        };

        Because of =
            () =>
            {
                foreach (var eventToPublish in availableDescendants)
                {
                    var publishedEventClosedType = typeof(PublishedEvent<>).MakeGenericType(eventToPublish);
                    var handleMethod = typeof(InterviewHistoryDenormalizer).GetMethod("Update", new[] { typeof(InterviewHistoryView), publishedEventClosedType });

                    if (handleMethod == null)
                        missingHandlers.Add(eventToPublish);
                }
            };

        It should_each_QuestionAnswered_descendant_be_handled_by_InterviewHistoryDenormalizer = () =>
            missingHandlers.ShouldBeEmpty();

        private static HashSet<Type> missingHandlers = new HashSet<Type>();

        private static List<Type> availableDescendants=new List<Type>();
    }
}
