using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Microsoft.Practices.ServiceLocation;
using Moq;

namespace WB.UI.Designer.Tests
{
    public class AssemblyContext : IAssemblyContext
    {
        public void OnAssemblyStart()
        {
            var serviceLocator = new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object;

            ServiceLocator.SetLocatorProvider(() => serviceLocator);
        }

        public void OnAssemblyComplete()
        {
        }
    }
}
