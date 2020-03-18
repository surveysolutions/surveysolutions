using System;
using WB.Core.Infrastructure.CommandBus;

namespace WB.UI.Headquarters.Code.CommandTransformation
{
    internal class IntreviewCommand : ICommand
    {
        public Guid UserId { get; set; }
        public Guid CommandIdentifier { get; set; }
        public long? KnownVersion { get; set; }
    }
}