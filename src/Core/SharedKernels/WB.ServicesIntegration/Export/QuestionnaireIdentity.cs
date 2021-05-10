using System;

namespace WB.ServicesIntegration.Export
{
    public class QuestionnaireIdentity
    {
        public QuestionnaireIdentity()
        {
        }

        public QuestionnaireIdentity(string id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            var split = id.Split("$");
            if (split.Length != 2) throw new ArgumentException("Questionnaire id is malformed", nameof(id))
            {
                Data = { {"id", id}}
            };
            
            this.Id = Guid.Parse(split[0]);
            this.Version = long.Parse(split[1]);
        }

        public QuestionnaireIdentity(Guid id, long version)
        {
            Id = id;
            Version = version;
        }

        public Guid Id { get; set; }
        
        public long Version { get; set; }

        public override string ToString()
        {
            return $"{Id:N}${Version}";
        }
    }
}
