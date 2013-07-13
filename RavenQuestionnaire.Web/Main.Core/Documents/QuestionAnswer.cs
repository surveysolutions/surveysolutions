using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;

namespace Main.Core.Documents
{
    public class QuestionAnswer
    {
        public Guid Id { get; set; }
        public QuestionType Type { get; set; }
        public string Answer { get; set; }
        public Guid[] Answers { get; set; }
    }
}
