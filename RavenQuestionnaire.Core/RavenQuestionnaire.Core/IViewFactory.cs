using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RavenQuestionnaire.Core
{
    public interface IViewFactory<TInput, TOutput>
    {
        TOutput Load(TInput input);
    }
}
