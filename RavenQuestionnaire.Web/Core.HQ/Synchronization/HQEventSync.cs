using Main.DenormalizerStorage;

namespace Core.HQ.Synchronization
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Main.Core.Documents;
    using Main.Core.Domain;
    using Main.Core.Events;
    using Ncqrs;
    using Ncqrs.Eventing.Storage;

    public class HQEventSync : AbstractSnapshotableEventSync
    {

        #region Fields

        /// <summary>
        /// ViewRepository  object
        /// </summary>
        private readonly IDenormalizer denormalizer;

        /// <summary>
        /// myEventStore object
        /// </summary>
        private readonly IEventStore myEventStore;

        #endregion

        #region Constructor

        public HQEventSync(IDenormalizer denormalizer)
        {
            this.denormalizer = denormalizer;
            this.myEventStore = NcqrsEnvironment.Get<IEventStore>();
            if (myEventStore == null)
                throw new Exception("IEventStore is not correct.");
        }

        #endregion

        #region OverrideMethods

        public override IEnumerable<AggregateRootEvent> ReadEvents()
        {
            var retval = new List<AggregateRootEvent>();
            this.AddQuestionnairesTemplates(retval);
            return retval.OrderBy(x => x.EventTimeStamp).ToList();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Responsible for added questionnaire templates
        /// </summary>
        /// <param name="retval">
        /// The retval.
        /// </param>
        private void AddQuestionnairesTemplates(List<AggregateRootEvent> retval)
        {
            var model = this.denormalizer.Query<QuestionnaireDocument>();

            foreach (var item in model)
            {
                retval.AddRange(this.GetEventStreamById(item.PublicKey, typeof(QuestionnaireAR)));
            }
        }

        #endregion
    }
}
