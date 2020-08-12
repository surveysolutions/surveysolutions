namespace WB.Services.Export.InterviewDataStorage
{
    public class CreateTableColumnInfo
    {
        public CreateTableColumnInfo(string name, string sqlType, bool isPrimaryKey = false, bool isNullable = false, string? defaultValue = null)
        {
            Name = name;
            SqlType = sqlType;
            IsPrimaryKey = isPrimaryKey;
            DefaultValue = defaultValue;
            IsNullable = isNullable;
        }

        public string Name { get; }
        public string SqlType { get; }
        public bool IsPrimaryKey { get; }
        public string? DefaultValue { get; }
        public bool IsNullable { get; }
    }
}
