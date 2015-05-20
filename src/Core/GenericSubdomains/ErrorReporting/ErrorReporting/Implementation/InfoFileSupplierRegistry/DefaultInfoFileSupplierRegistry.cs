using System;
using System.Collections.Generic;

using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.Core.GenericSubdomains.ErrorReporting.Implementation.InfoFileSupplierRegistry
{
    internal class DefaultInfoFileSupplierRegistry : IInfoFileSupplierRegistry
    {
        private readonly List<string> infoFilePaths = new List<string>();
        private readonly List<Func<string>> infoFilePathsCallback = new List<Func<string>>();

        public void RegisterConstant(string infoFilePath)
        {
            this.infoFilePaths.Add(infoFilePath);
        }

        public void Register(Func<string> infoFilePathCallback)
        {
            this.infoFilePathsCallback.Add(infoFilePathCallback);
        }

        public IEnumerable<string> GetAll()
        {
            foreach (var infoFilePath in this.infoFilePaths)
            {
                yield return infoFilePath;
            }
            foreach (var infoFilePathCallback in this.infoFilePathsCallback)
            {
                yield return infoFilePathCallback();
            }
        }
    }
}