using System;

namespace WB.ServicesIntegration.Export
{
    public class Categories
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = String.Empty;

        public CategoryItem[] Values { get; set; } = new CategoryItem[0];
    }

    public class CategoryItem
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public string Text { get; set; } = String.Empty;
    }
}
