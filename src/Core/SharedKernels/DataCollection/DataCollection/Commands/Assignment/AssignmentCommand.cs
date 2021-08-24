using System;
using Ncqrs.Commanding;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.SharedKernels.DataCollection.Commands.Assignment
{
    public abstract class AssignmentCommand : CommandBase
    {
        public Guid PublicKey { get; }
        
        public QuestionnaireIdentity QuestionnaireId { get; }

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

        protected AssignmentCommand(Guid publicKey, Guid userId, QuestionnaireIdentity questionnaireId)
            : base(publicKey)
        {
            this.PublicKey = publicKey;
            this.UserId = userId;
            this.QuestionnaireId = questionnaireId;

            this.OriginDate = DateTimeOffset.Now;
        }

        public DateTimeOffset OriginDate { get; }
    }
}
