namespace Main.Core.Events.Questionnaire
{
    using System;

    public class ImageUploaded
    {
        public string Description { get; set; }

        public Guid ImagePublicKey { get; set; }

        public Guid PublicKey { get; set; }

        public string Title { get; set; }
    }
}