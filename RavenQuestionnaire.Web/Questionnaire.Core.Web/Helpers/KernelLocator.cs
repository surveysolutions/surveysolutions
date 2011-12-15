using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;

namespace Questionnaire.Core.Web.Helpers
{

    public static class KernelLocator
    {
        public static void SetKernel(IKernel kernel)
        {
            _kernel = kernel;
        }
        public static IKernel Kernel
        {
            get { return _kernel; }
        }
        private static IKernel _kernel;
    }
}
