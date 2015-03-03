using System;
using System.Collections.Generic;
using Microsoft.Practices.ServiceLocation;
using Ncqrs.Commanding;
using Ncqrs.Config;
using Ncqrs.Domain;
using Ncqrs.Domain.Storage;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using Ncqrs.Eventing.Storage;

namespace Ncqrs
{
    /// <summary>The Ncqrs environment. This class gives access to other components registered in this environment.
    /// <remarks>
    /// Make sure to call the <see cref="Configure"/> method before doing anything else with this class.
    /// </remarks></summary>
    public static class NcqrsEnvironment
    {
        static NcqrsEnvironment()
        {
            //InitDefaults();
        }

        /// <summary>
        /// Initialize defaults with default components.
        /// </summary>
        public static void InitDefaults()
        {
            // Initialize defaults.
            SetDefault<IClock>(new DateTimeBasedClock());
            var eventStore = new InMemoryEventStore();
            SetDefault<IEventBus>(new InProcessEventBus(eventStore));
            SetDefault<IEventStore>(eventStore);
            SetDefault<ISnapshotStore>(new NullSnapshotStore());
            SetDefault<ISnapshottingPolicy>(new NoSnapshottingPolicy());
            SetDefault<IAggregateRootCreationStrategy>(new SimpleAggregateRootCreationStrategy());
            SetDefault<IAggregateSupportsSnapshotValidator>(new AggregateSupportsSnapshotValidator());
            SetGetter<IAggregateSnapshotter>(() => new DefaultAggregateSnapshotter(
                Get<IAggregateRootCreationStrategy>(), Get<IAggregateSupportsSnapshotValidator>(), Get<ISnapshottingPolicy>(), Get<ISnapshotStore>()));
        }

        /// <summary>
        /// Holds the defaults for requested types that are not configured.
        /// </summary>
        /// <remarks>
        /// Use the <see cref="SetDefault{T}"/> method to set a default.
        /// </remarks>
        private static readonly Dictionary<Type, Object> _defaults = new Dictionary<Type, object>(0);

        private static readonly Dictionary<Type, Func<object>> _getters = new Dictionary<Type, Func<object>>();

        /// <summary>
        /// Hold the environment configuration. This is initialized by the <see cref="Configure"/> method.
        /// </summary>
        private static IEnvironmentConfiguration _instance;

        private static readonly Dictionary<string, Type> KnownEventDataTypes = new Dictionary<string, Type>();

        /// <summary>
        /// Gets or create the requested instance specified by the parameter <typeparamref name="T"/>.
        /// </summary>
        /// <remarks>This</remarks>
        /// <typeparam name="T">The type of the instance that is requested.
        /// </typeparam>
        /// <returns>If the type specified by <typeparamref name="T"/> is
        /// registered, it returns an instance that is, is a super class of, or
        /// implements the type specified by <typeparamref name="T"/>. Otherwise
        /// a <see cref="InstanceNotFoundInEnvironmentConfigurationException"/>
        /// occurs.
        /// </returns>
        public static T Get<T>() where T : class
        {
            T result = null;

            if (_instance == null || !_instance.TryGet(out result))
            {
                object defaultResult;
                Func<object> getter;

                if (_getters.TryGetValue(typeof(T), out getter))
                {
                    result = ((Func<T>) getter).Invoke();
                }
                else if (_defaults.TryGetValue(typeof(T), out defaultResult))
                {
                    result = (T)defaultResult;
                }
            }

            if(result == null)
                throw new InstanceNotFoundInEnvironmentConfigurationException(typeof(T));

            return result;
        }

        /// <summary>
        /// Sets the default for an type. This default instance is returned when
        /// the configured <see cref="IEnvironmentConfiguration"/> did not
        /// returned an instance for this type.
        /// </summary>
        /// <remarks>When the type already contains a default, it is overridden.
        /// </remarks>
        /// <typeparam name="T">The type of the instance to set a default.
        /// </typeparam>
        /// <param name="instance">The instance to set as default.</param>
        public static void SetDefault<T>(T instance) where T : class
        {
            _defaults[typeof(T)] = instance;
        }

        public static void SetGetter<T>(Func<T> getter) where T : class
        {
            _getters[typeof(T)] = getter;
        }

        /// <summary>
        /// Removes the default for specified type.
        /// </summary>
        /// <remarks>When there is no default set, this action is ignored.</remarks>
        /// <typeparam name="T">The registered default type.</typeparam>
        public static void RemoveDefault<T>() where T : class
        {
            _defaults.Remove(typeof(T));
        }

        /// <summary>
        /// Configures the Ncqrs environment.
        /// </summary>
        /// <param name="source">The source that contains the configuration for the current environment.</param>
        public static void Configure(IEnvironmentConfiguration source)
        {
            _instance = source;
        }

        /// <summary>
        /// When the environment is configured it removes the configuration. Defaults are not removed.
        /// </summary>
        public static void Deconfigure()
        {
            _instance = null;
            _defaults.Clear();
            _getters.Clear();

            InitDefaults();
        }

        /// <summary>
        /// Gets a value indicating whether this environment is configured.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this environment is configured; otherwise, <c>false</c>.
        /// </value>
        public static Boolean IsConfigured
        {
            get
            {
                return _instance != null;
            }
        }

        /// <summary>
        /// Returns the current environment configuration
        /// </summary>
        /// <remarks>
        /// Returns the current environment configuration, or null if not configured
        /// </remarks>
        public static IEnvironmentConfiguration CurrentConfiguration { get { return _instance; } }

        public static void RegisterEventDataType(Type eventDataType)
        {
            ThrowIfThereIsAnotherEventWithSameFullName(eventDataType);
            ThrowIfThereIsAnotherEventWithSameName(eventDataType);

            KnownEventDataTypes[eventDataType.FullName] = eventDataType;
            KnownEventDataTypes[eventDataType.Name] = eventDataType;
        }

        public static bool IsEventDataType(string typeFullName)
        {
            return KnownEventDataTypes.ContainsKey(typeFullName);
        }

        public static Type GetEventDataTypeByFullName(string typeFullName)
        {
            return KnownEventDataTypes[typeFullName];
        }

        public static Type GetEventDataTypeByName(string typeName)
        {
            return KnownEventDataTypes[typeName];
        }

        private static void ThrowIfThereIsAnotherEventWithSameName(Type @event)
        {
            Type anotherEventWithSameName;
            KnownEventDataTypes.TryGetValue(@event.Name, out anotherEventWithSameName);

            if (anotherEventWithSameName != null && anotherEventWithSameName != @event)
                throw new ArgumentException(string.Format("Two different events share same type name:{0}{1}{0}{2}",
                    Environment.NewLine, @event.AssemblyQualifiedName, anotherEventWithSameName.AssemblyQualifiedName));
        }

        private static void ThrowIfThereIsAnotherEventWithSameFullName(Type @event)
        {
            Type anotherEventWithSameName;
            KnownEventDataTypes.TryGetValue(@event.FullName, out anotherEventWithSameName);

            if (anotherEventWithSameName != null && anotherEventWithSameName != @event)
                throw new ArgumentException(string.Format(
                    "Two different events share same full type name:{0}{1}{0}{2}",
                    Environment.NewLine, @event.AssemblyQualifiedName, anotherEventWithSameName.AssemblyQualifiedName));
        }
    }
}