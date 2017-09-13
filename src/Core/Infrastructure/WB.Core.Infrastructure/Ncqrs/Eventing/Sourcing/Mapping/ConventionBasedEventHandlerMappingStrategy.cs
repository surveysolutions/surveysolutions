using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Ncqrs.Eventing.Sourcing.Mapping
{
    /// <summary>
    /// A internal event handler mapping strategy that maps methods as an event handler based on method name and parameter type.
    /// <remarks>
    /// All method that match the following requirements are mapped as an event handler:
    /// <list type="number">
    ///     <item>
    ///         <value>
    ///             Method name should start with <i>On</i> or <i>on</i>. Like: <i>OnProductAdded</i> or <i>onProductAdded</i>.
    ///         </value>
    ///     </item>
    ///     <item>
    ///         <value>
    ///             The method should only accept one parameter.
    ///         </value>
    ///     </item>
    ///     <item>
    ///         <value>
    ///             The parameter must be, or implemented from, the type specified by the <see cref="EventBaseType"/> property. Which is <see cref="object"/> by default.
    ///         </value>
    ///     </item>
    /// </list>
    /// </remarks>
    /// </summary>
    public class ConventionBasedEventHandlerMappingStrategy : IEventHandlerMappingStrategy
    {
        public Type EventBaseType { get; set; }
        private static readonly Regex MethodNamePattern = new Regex("^(on|On|ON)+|Apply$", RegexOptions.CultureInvariant | RegexOptions.Compiled);

        private static readonly ConcurrentDictionary<MethodInfo, ParameterInfo[]> _parametersCache = new ConcurrentDictionary<MethodInfo, ParameterInfo[]>();

        public ConventionBasedEventHandlerMappingStrategy()
        {
            EventBaseType = typeof(Object);
        }

        static readonly ParameterInfo[] EmptyParameters = Array.Empty<ParameterInfo>();

        private ParameterInfo[] GetParameters(MethodInfo method)
        {
            return _parametersCache.GetOrAdd(method, m =>
            {
                var res = m.GetParameters();
                if(res.Length == 0)
                {
                    return EmptyParameters;
                }
                return res;
            });
        }

        public IEnumerable<ISourcedEventHandler> GetEventHandlers(object target)
        {
            var targetType = target.GetType();

            var methodsToMatch = GetPotentialApplyMethods(targetType);

            IEnumerable<(MethodInfo MethodInfo, ParameterInfo FirstParameter)> matchedMethods = 
                from method in methodsToMatch
                let parameters = GetParameters(method)
                where
                    // Get only methods where the name matches.
                    MethodNamePattern.IsMatch(method.Name) &&
                    // Get only methods that have 1 parameter.
                    parameters.Length == 1 &&
                    // Get only methods where the first parameter is an event.
                    EventBaseType.GetTypeInfo().IsAssignableFrom(parameters[0].ParameterType.GetTypeInfo())
                    // Get only methods that are not marked with the no event handler attribute.
                select (method, parameters[0]);

            foreach (var method in matchedMethods)
            {
                var firstParameterType = method.FirstParameter.ParameterType;

                void InvokeAction(object e) => method.MethodInfo.Invoke(target, new[] {e});

                yield return new TypeThresholdedActionBasedDomainEventHandler(InvokeAction, firstParameterType, method.MethodInfo.Name, true);
            }
        }

        public bool CanHandleEvent(object target, Type committedEvent)
        {
            var targetType = target.GetType();
            var methodsToMatch = GetPotentialApplyMethods(targetType);

            for (var index = 0; index < methodsToMatch.Length; index++)
            {
                var method = methodsToMatch[index];
                var parameters = GetParameters(method);

                if ( // Get only methods where the name matches.
                    MethodNamePattern.IsMatch(method.Name) &&
                    // Get only methods that have 1 parameter.
                    parameters.Length == 1 &&
                    // Get only methods where the first parameter is equal to event.
                    committedEvent.GetTypeInfo().IsAssignableFrom(parameters[0].ParameterType.GetTypeInfo()))
                {
                    return true;
                }
            }

            return false;
        }

        private static MethodInfo[] GetPotentialApplyMethods(Type targetType)
        {
            return targetType.GetTypeInfo().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        }
    }
}