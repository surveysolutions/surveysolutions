namespace WB.UI.Headquarters.Models
{
  
    public class PagerData
    {
        
        public PagerData()
        {
            this.Page = 1;
            this.PageSize = 20;
        }
       
        public int Page { get; set; }

        public int PageSize { get; set; }
    }
}