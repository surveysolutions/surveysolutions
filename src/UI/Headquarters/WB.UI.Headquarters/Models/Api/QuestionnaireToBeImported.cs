﻿using System;

namespace WB.UI.Headquarters.Models.Api
{
    public class QuestionnaireToBeImported
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string LastModified { get; set; }
        public string CreatedBy { get; set; }
    }
}