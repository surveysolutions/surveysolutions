using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ninject.Components;
using Ninject.Infrastructure;
using Ninject.Infrastructure.Language;

namespace Ninject.Modules
{
	public class AndroidModuleLoader : NinjectComponent, IModuleLoader
	{
		public IKernel Kernel { get; private set; }

		public AndroidModuleLoader(IKernel kernel)
		{
			Ensure.ArgumentNotNull(kernel, "kernel");
			Kernel = kernel;
		}

		public void LoadModules(IEnumerable<string> patterns)
		{
			var assemblies = AppDomain.CurrentDomain
				.GetAssemblies()
				.Where(HasNinjectModules)
				.ToList();

			Kernel.Load(assemblies);
		}

		private bool HasNinjectModules(Assembly assembly)
		{
			if (!assembly.FullName.ToLowerInvariant().Contains("ninject.extensions"))
				return false;

			return assembly.GetTypes()
				.Any(type => typeof (INinjectModule).IsAssignableFrom(type)
				             && !type.IsAbstract
				             && !type.IsInterface
				             && type.GetConstructor(Type.EmptyTypes) != null);
		}
	}
}