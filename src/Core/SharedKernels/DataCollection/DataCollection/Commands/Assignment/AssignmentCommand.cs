using System;
using Ncqrs.Commanding;

namespace WB.Core.SharedKernels.DataCollection.Commands.Assignment
{
    public abstract class AssignmentCommand : CommandBase
    {
        public Guid PublicKey { get; }

        private Guid userId;
        public Guid UserId
        {
            get
            {
                if (this.userId == Guid.Empty)
                    throw new ArgumentException("User ID cannot be empty.");

                return this.userId;
            }

            private set => this.userId = value;
        }

        protected AssignmentCommand(Guid publicKey, Guid userId)
            : base(publicKey)
        {
            this.PublicKey = publicKey;
            this.UserId = userId;

            this.OriginDate = DateTimeOffset.Now;
        }

        public DateTimeOffset OriginDate { get; }
    }
}
