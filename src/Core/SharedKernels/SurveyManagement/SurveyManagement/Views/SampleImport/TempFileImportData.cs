using System;
using System.Collections.Generic;
using System.Linq;

namespace WB.Core.SharedKernels.SurveyManagement.Views.SampleImport
{
    internal class TempFileImportData
    {

        public void CompleteImport()
        {
            this.IsCompleted = true;
        }
        
        public void AddValueBatch(string[][] values)
        {
            if(this.Values==null)
                this.Values=new List<string[]>();
            values = values.Where(row => row.Any(item => !string.IsNullOrEmpty(item))).ToArray();
            for (int i = 0; i < values.Length; i++)
            {
                this.Values.Add(values[i]);
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
