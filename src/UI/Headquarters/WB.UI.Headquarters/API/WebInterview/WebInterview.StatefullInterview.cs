using System;
using System.Collections.Generic;
using System.Globalization;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.UI.Headquarters.API.WebInterview
{
    public partial class WebInterview
    {
        public string GetTitleText(Identity entityIdentity) 
            => this.CurrentInterview.GetTitleText(entityIdentity);

    }
}