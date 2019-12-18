using System;
using System.Collections.Generic;

namespace WB.Services.Export.Questionnaire
{
    public class Categories
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public IEnumerable<CategoryItem> Values { get; set; }
    }

    public class CategoryItem
    {
        public string Title { get; set; }
        public int Value { get; set; }
    }
}
