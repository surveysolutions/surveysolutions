﻿using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.Classifications
{
    public interface IClassificationEntity
    {
        Guid Id { get; }
        string Title { get; }
    }

    public class Category : IClassificationEntity
    {
        public Guid Id { get; set; }
        public int Value { get; set; }
        public string Title { get;set; }
        public int Order { get; set; }
        public Guid? Parent{ get;set; }
    }

    public class Classification : IClassificationEntity
    {
        public Guid Id { get; set;}
        public string Title { get;set; }
        public Guid? Parent { get;set; }
        public Guid? UserId { get;set; }
        public ClassificationGroup Group { get; set; }
        public int CategoriesCount { get; set; }
    }

    public class ClassificationGroup : IClassificationEntity
    {
        public Guid Id { get; set;}
        public string Title { get; set;}
    }

    public class MysqlClassificationEntity
    {
        public int Id { get; set; }
        public int Original_id { get; set; }
        public Guid IdGuid { get; set; }
        public string Title { get; set; }
        public int? Parent { get; set; }
        public Guid? ParentGuid { get; set; }
        public ClassificationEntityType Type { get; set; }
        public int? Value { get; set; }
        public int? Order { get; set; }
        public Guid? ClassificationId { get; set; }
    }

    public enum ClassificationEntityType
    {
        Group = 1,
        Classification = 2,
        Category = 3
    }

    public class ClassificationsSearchResult
    {
        public List<Classification> Classifications { get; set; }
        public int Total { get; set; }
    }
}
