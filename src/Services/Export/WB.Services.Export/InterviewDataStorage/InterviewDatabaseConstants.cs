namespace WB.Services.Export.InterviewDataStorage
{
    public class InterviewDatabaseConstants
    {
        public const string InterviewId = "interview__id";
        public const string RosterVector = "roster__vector";

        public class SqlTypes
        {
            public const string Guid = "uuid";
            public const string Integer = "int4";
            public const string Double = "float8";
            public const string Long = "int8";
            public const string IntArray = "int4[]";
            public const string Jsonb = "jsonb";
            public const string Bool = "bool";
            public const string String = "text";
            public const string DateTime = "timestamp";
        }
    }
}
