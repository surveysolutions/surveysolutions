using System;

// ReSharper disable once CheckNamespace
namespace WB.Core.SharedKernels.SurveySolutions.Api.Designer
{
    public class QuestionnaireListItem
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public DateTime LastEntryDate { get; set; }
        public bool IsPublic { get; set; }
        public string Owner { get; set; }
        public bool IsShared { get; set; }
    }
}
