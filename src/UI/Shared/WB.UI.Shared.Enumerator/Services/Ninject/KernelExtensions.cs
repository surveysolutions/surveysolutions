using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Ninject;
using Ninject.Infrastructure;
using Ninject.Planning.Bindings;
using WB.Core.GenericSubdomains.Utils;

namespace WB.UI.Shared.Enumerator.Services.Ninject
{
    public static class KernelExtensions
    {
        public static void VerifyIfDebug(this IKernel kernel)
        {
            #if DEBUG

            Verify((KernelBase) kernel);

            #endif
        }

        private static void Verify(KernelBase kernel)
        {
            IEnumerable<Type> typesToVerify = kernel
                .GetAllRegisteredTypes()
                .Where(type => !type.ContainsGenericParameters);

            try
            {
                ActionUtils.ExecuteInIndependentTryCatchBlocks(
                    typesToVerify,
                    type => kernel.ResolveTypeOrThrowArgumentException(type));
            }
            catch (AggregateException aggregateException)
            {
                var errors = aggregateException
                    .InnerExceptions
                    .Cast<ArgumentException>()
                    .Select(argumentException => string.Format("{0} - {1}", argumentException.ParamName, ShortenExceptionMessage(argumentException.Message)))
                    .ToList();

                var message = string.Format(
                    "Failed to resolve {1} following non-generic types:{0}{2}",
                    Environment.NewLine,
                    errors.Count,
                    string.Join(Environment.NewLine, errors));

                Debug.WriteLine(message);

                throw new Exception(message, aggregateException);
            }
        }

        private static string ShortenExceptionMessage(string message)
        {
            return string.Join(
                Environment.NewLine,
                message
                    .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                    .Take(2));
        }

        private static void ResolveTypeOrThrowArgumentException(this KernelBase kernel, Type type)
        {
            try
            {
                kernel.Get(type);
            }
            catch (Exception exception)
            {
                throw new ArgumentException(exception.Message, type.Name, exception);
            }
        }

        private static Type[] GetAllRegisteredTypes(this KernelBase kernel)
        {
            var bindings = GetPrivateFieldValue<KernelBase, Multimap<Type, IBinding>>(kernel, "bindings");

            return bindings.Select(x => x.Key).ToArray();
        }

        private static TValue GetPrivateFieldValue<TInstance, TValue>(TInstance instance, string fieldName)
        {
            return (TValue)
                typeof(TInstance)
                    .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetValue(instance);
        }
    }
}