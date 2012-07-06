using System;

namespace RavenQuestionnaire.Core.Entities.SubEntities
{
    public class SurveyStatus
    {
        public SurveyStatus()
        {
        }

        public SurveyStatus(Guid id)
        {
            PublicId = id;
        }

        public SurveyStatus(Guid id, string name)
        {
            PublicId = id;
            Name = name;
        }

        public Guid PublicId { get; set; }
        public string Name { get; set; }
        public string ChangeComment { get; set; }


        public static SurveyStatus Initial 
        {
            get
            {
                Guid identifier = new Guid("8927D124-3CFB-4374-AD36-2FD99B62CE13");
                string name = "Initial";
                return new SurveyStatus(identifier, name); 
            } 
        }

        public static SurveyStatus Error
        {
            get
            {
                Guid identifier = new Guid("D65CF1F6-8A75-43FA-9158-B745EB4D6A1F");
                string name = "Error";
                return new SurveyStatus(identifier, name);
            }
        }

        public static SurveyStatus Complete
        {
            get
            {
                Guid identifier = new Guid("776C0DC1-23C4-4B03-A3ED-B24EF005559B");
                string name = "Complete";
                return new SurveyStatus(identifier, name);
            }
        }


    }
}
