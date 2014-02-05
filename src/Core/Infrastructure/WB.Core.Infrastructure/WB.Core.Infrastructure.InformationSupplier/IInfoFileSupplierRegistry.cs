using System;
using System.Collections.Generic;

namespace WB.Core.Infrastructure.InformationSupplier
{
    public interface IInfoFileSupplierRegistry
    {
        void RegisterConstant(string infoFilePath);
        void Register(Func<string> infoFilePathCallback);
        IEnumerable<string> GetAll();
    }
}