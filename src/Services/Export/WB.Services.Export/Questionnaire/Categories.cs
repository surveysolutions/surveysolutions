using System;
using System.Collections.Generic;

namespace WB.Services.Export.Questionnaire
{
    public class Categories
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public CategoryItem[] Values { get; set; }
    }

    public class CategoryItem
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public string Text { get; set; }
    }
}
