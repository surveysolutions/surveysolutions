using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using Main.DenormalizerStorage;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ninject;
using Ninject.Activation;
using Ninject.Extensions.Conventions;
using Ninject.Extensions.Conventions.BindingGenerators;
using Ninject.Modules;
using Ninject.Syntax;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.GenericSubdomains.Utils.Implementation;
using WB.Core.GenericSubdomains.Utils.Rest;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Security;
using WB.UI.Headquarters.Views;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Headquarters.Injections
{
    public class RegisterFirstInstanceOfInterface : IBindingGenerator
    {
        public RegisterFirstInstanceOfInterface(IEnumerable<Assembly> assemblyes)
        {
            this._assemblyes = assemblyes;
        }

        private readonly IEnumerable<Assembly> _assemblyes;

        public IEnumerable<IBindingWhenInNamedWithOrOnSyntax<object>> CreateBindings(
            Type type, IBindingRoot bindingRoot)
        {
            IEnumerable<IBindingWhenInNamedWithOrOnSyntax<object>> y =
                Enumerable.Empty<IBindingWhenInNamedWithOrOnSyntax<object>>();

            if (!type.IsInterface)
            {
                return y;
            }


            if (type.IsGenericType)
            {
                return y;
            }

            Type matchedType = this._assemblyes
                .SelectMany(a => a.GetTypes().Where(t => t.IsVisible))
                .FirstOrDefault(
                    x => !x.IsAbstract && x.GetInterface(type.FullName) != null);
            if (matchedType == null)
            {
                return y;
            }

            return new[] { bindingRoot.Bind(new[] { type }).To(matchedType) };
        }
    }

    public abstract class CoreRegistry : NinjectModule
    {
        protected virtual IEnumerable<Assembly> GetAssembliesForRegistration()
        {
            return new[] { (typeof(CoreRegistry)).Assembly };
        }

        /// <summary>
        /// Gets pairs of interface/type which should be registered.
        /// Usually is used to return implementation of interfaces declared not in assemblies returned by GetAssemblies method.
        /// </summary>
        /// <returns>Pairs of interface/implementation.</returns>
        protected virtual IEnumerable<KeyValuePair<Type, Type>> GetTypesForRegistration()
        {
            return Enumerable.Empty<KeyValuePair<Type, Type>>();
        }

        public override void Load()
        {
            RegisterDenormalizers();
            RegisterEventHandlers();
            RegisterAdditionalElements();
        }

        protected virtual void RegisterAdditionalElements()
        {
            this.Kernel.Bind(
                x =>
                x.From(this.GetAssembliesForRegistration()).SelectAllInterfaces().BindWith(
                    new RegisterFirstInstanceOfInterface(this.GetAssembliesForRegistration())));

            foreach (KeyValuePair<Type, Type> customBindType in this.GetTypesForRegistration())
            {
                this.Kernel.Bind(customBindType.Key).To(customBindType.Value);
            }
        }

        protected virtual void RegisterViewFactories()
        {
            BindInterface(this.GetAssembliesForRegistration(), typeof(IViewFactory<,>), (c) => Guid.NewGuid());
        }

        protected virtual void RegisterEventHandlers()
        {
            BindInterface(this.GetAssembliesForRegistration(), typeof(IEventHandler<>), (c) => this.Kernel);
        }

        protected virtual void RegisterDenormalizers()
        {
            // currently in-memory repo accessor also contains repository itself as internal dictionary, so we need to create him as singletone
            this.Kernel.Bind(typeof(InMemoryReadSideRepositoryAccessor<>)).ToSelf().InSingletonScope();

            this.Kernel.Bind(typeof(IReadSideRepositoryReader<>)).ToMethod(this.GetInMemoryReadSideRepositoryAccessor);
            this.Kernel.Bind(typeof(IQueryableReadSideRepositoryReader<>)).ToMethod(this.GetInMemoryReadSideRepositoryAccessor);
            this.Kernel.Bind(typeof(IReadSideRepositoryWriter<>)).ToMethod(this.GetInMemoryReadSideRepositoryAccessor);
        }

        protected object GetInMemoryReadSideRepositoryAccessor(IContext context)
        {
            var genericParameter = context.GenericArguments[0];

            return this.Kernel.Get(typeof(InMemoryReadSideRepositoryAccessor<>).MakeGenericType(genericParameter));
        }

        protected void BindInterface(IEnumerable<Assembly> assembyes, Type interfaceType, Func<IContext, object> scope)
        {

            var implementations =
             assembyes.SelectMany(a => a.GetTypes()).Where(t => t.IsPublic && ImplementsAtLeastOneInterface(t, interfaceType));
            foreach (Type implementation in implementations)
            {
                if (interfaceType != typeof(IViewFactory<,>))
                {
                    this.Kernel.Bind(interfaceType).To(implementation).InScope(scope);
                }
                if (interfaceType.IsGenericType)
                {
                    var interfaceImplementations =
                        implementation.GetInterfaces().Where(i => IsInterfaceInterface(i, interfaceType));
                    foreach (Type interfaceImplementation in interfaceImplementations)
                    {
                        this.Kernel.Bind(interfaceType.MakeGenericType(interfaceImplementation.GetGenericArguments())).
                            To(implementation).InScope(scope);
                    }
                }
            }
        }

        private bool ImplementsAtLeastOneInterface(Type type, Type interfaceType)
        {
            return type.IsClass && !type.IsAbstract &&
                   type.GetInterfaces().Any(i => IsInterfaceInterface(i, interfaceType));
        }

        private bool IsInterfaceInterface(Type type, Type interfaceType)
        {
            return type.IsInterface
                && ((interfaceType.IsGenericType && type.IsGenericType && type.GetGenericTypeDefinition() == interfaceType)
                    || (!type.IsGenericType && !interfaceType.IsGenericType && type == interfaceType));
        }
    }
    public class HeadquartersRegistry : CoreRegistry
    {
        protected override IEnumerable<Assembly> GetAssembliesForRegistration()
        {
            return base.GetAssembliesForRegistration().Concat(new[]
            {
                typeof(UserViewFactory).Assembly,
                typeof(QuestionnaireMembershipProvider).Assembly,
                typeof(QuestionnaireItemInputModel).Assembly,
                typeof(HeadquartersBoundedContextModule).Assembly
            });
        }

        protected override void RegisterDenormalizers() { }

        protected override IEnumerable<KeyValuePair<Type, Type>> GetTypesForRegistration()
        {
            var supervisorSpecificTypes = new Dictionary<Type, Type>
            {
                { typeof(IExceptionFilter), typeof(HandleUIExceptionAttribute) }
            };

            return base.GetTypesForRegistration().Concat(supervisorSpecificTypes);
        }
        protected override void RegisterEventHandlers()
        {
            base.RegisterEventHandlers();

            this.BindInterface(this.GetAssembliesForRegistration(), typeof(IEventHandler), (c) => this.Kernel);
        }

        public override void Load()
        {
            base.Load();

            this.RegisterViewFactories();

            this.Bind<JsonUtilsSettings>().ToSelf().InSingletonScope();
            this.Bind<IJsonUtils>().To<NewtonJsonUtils>();
            this.Bind<IStringCompressor>().To<JsonCompressor>();
            this.Bind<IRestServiceSettings>().To<DesignerQuestionnaireApiRestServiceSettings>().InSingletonScope();
            this.Bind<IRestService>().To<RestService>().WithConstructorArgument("networkService", _ => null);
        }
    }
}