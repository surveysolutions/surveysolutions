using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.SharedKernels.DataCollection.Services
{
    public class TempFileImportData:IView
    {

        public void CompleteImport()
        {
            IsCompleted = true;
        }
        
        public void AddValueBatch(string[][] values)
        {
            if(Values==null)
                this.Values=new List<string[]>();
            for (int i = 0; i < values.Length; i++)
            {
                Values.Add(values[i]);
            }
        }

        public Guid PublicKey { get;  set; }
        public Guid TemplateId { get;  set; }
        public List<string[]> Values { get;  set; }
        public string[]  Header { get;  set; }
        public bool IsCompleted { get; set; }
        public string ErrorMassage { get; set; }
    }
}
