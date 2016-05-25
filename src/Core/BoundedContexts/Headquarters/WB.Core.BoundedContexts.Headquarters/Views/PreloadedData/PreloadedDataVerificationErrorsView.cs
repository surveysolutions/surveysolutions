﻿using System;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.PreloadedData;

namespace WB.Core.BoundedContexts.Headquarters.Views.PreloadedData
{
    public class PreloadedDataVerificationErrorsView
    {
        public PreloadedDataVerificationErrorsView(Guid questionnaireId, long version, string questionnaireTitle, PreloadedDataVerificationError[] errors, 
            bool wasSupervsorProvided,
            string id, PreloadedContentType preloadedContentType)
        {
            this.PreloadedContentType = preloadedContentType;
            this.Id = id;
            this.QuestionnaireId = questionnaireId;
            this.Version = version;
            this.Errors = errors;
            this.WasSupervsorProvided = wasSupervsorProvided;
            this.QuestionnaireTitle = questionnaireTitle;
        }

        public string Id { get; private set; }

        public Guid QuestionnaireId { get; private set; }

        public long Version { get; private set; }

        public PreloadedDataVerificationError[] Errors { get; private set; }

        public PreloadedContentType PreloadedContentType { get; private set; }

        public bool WasSupervsorProvided { get; set; }
        public string QuestionnaireTitle { get; set; }
    }
}
