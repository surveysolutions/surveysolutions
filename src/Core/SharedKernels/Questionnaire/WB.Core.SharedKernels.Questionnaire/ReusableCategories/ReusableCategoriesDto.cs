using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.Questionnaire.Categories;

namespace WB.Core.SharedKernels.SurveySolutions.ReusableCategories
{
    public class ReusableCategoriesDto
    {
        public Guid Id { get; set; }
        public List<CategoriesItem> Options { get; set; } = new List<CategoriesItem>();
    }
}
