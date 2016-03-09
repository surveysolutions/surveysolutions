namespace WB.Core.SharedKernels.SurveySolutions.Documents
{
    public class Macro
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Content { get; set; }

        public Macro Clone()
        {
            return new Macro
            {
                Name = this.Name,
                Description = this.Description,
                Content = this.Content
            };
        }
    }
}