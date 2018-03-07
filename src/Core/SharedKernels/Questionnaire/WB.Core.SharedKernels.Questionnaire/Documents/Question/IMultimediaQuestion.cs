using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Main.Core.Entities.SubEntities.Question
{
    public interface IMultimediaQuestion : IQuestion
    {
        bool IsSignature { get; }
    }
}
