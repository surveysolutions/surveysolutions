﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.Infrastructure.ReadSide;

namespace WB.Core.SharedKernels.SurveyManagement.Views.DataExport
{
    public class RecordFirstAnswerMarkerView : IView
    {
        public RecordFirstAnswerMarkerView(Guid interviewId)
        {
            this.InterviewId = interviewId;
        }

        public Guid InterviewId { get; private set; }
    }
}
