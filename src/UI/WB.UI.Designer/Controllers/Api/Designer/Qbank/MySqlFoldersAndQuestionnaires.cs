using System;

namespace WB.UI.Designer.Api.Designer.Qbank
{
    /*
     * [{"id":11, "classification_id":1, "classification_label":"Yes", "description":"", "value":"1", "is_missing":0, "weight":11, "attachments":""},
     */
    public class MySqlOptions
    {
        public int Id { get; set; }
        public int Classification_id { get; set; }
        public string Classification_label { get; set; } = String.Empty;
        public string Value { get; set; } = String.Empty;
        public int Weight { get; set; }
    }
    /*
   *{"id":344,
   * "alternateId":"b49d36b1-3517-44ab-b57e-fa5f260b2b99",
   * "pid":0,
   * "name":"Household Surveys - Packages",
   * "description":"Household survey \"packages\"",
   * "notes":"",
   * "published":1,
   * "weight":344},
   */
    public class MySqlFoldersAndQuestionnaires
    {
        public int Id { get; set; }
        public Guid IdGuid { get; set; }
        public int Pid { get; set; }
        public string Name { get; set; } = String.Empty;
        public string Description { get; set; } = string.Empty;
        public int Published { get; set; }
    }

    /*
   * {"id":1,
   * "quest_module_id":1009,
   * "classification_id":0,
   * "alternate_id":"ID_634030532775341459",
   * "name":"ID01",
   * "description":"Place name",
   * "pre_text":"PLACE NAME",
   * "literal_text":"",
   * "post_text":"",
   * "val_rep_format":"20",
   * "val_post_text":"a:0:{}",
   * "visual_rep_format":2,
   * "notes":"This section should be adapted for country-specific survey design.",
   * "instructions":"",
   * "universe":"",
   * "weight":1,
   * "published":0,
   * "created":"",
   * "changed":""},
   */
    public class MySqlQuestions
    {
        public int Id { get; set; }
        public int Quest_module_id { get; set; }
        public string? Name { get; set; }
        public string? Description{ get; set; }
        public string? Pre_text{ get; set; }
        public string? Post_text{ get; set; }
        public string? Literal_text{ get; set; }
        
        public int? Visual_rep_format{ get; set; }
        public string? Notes{ get; set; }
        public string? Instructions{ get; set; }
        public int? Published { get; set; }
        public int? Weight { get; set; }
        public int? Classification_id { get; set; }
        
    }

    /*
   *{"id":1009,
   * "alternate_id":"ID_634020960413702427",
   * "name":"AIS - Household Identification and Interviewer Visits",
   * "description":"The cover page records data about the location of the household, the date, and the outcome of the Household interview.",
   * "quality_control":"",
   * "notes":"Fill in the identification information in the box at the top of the cover page before an interviewer goes to a selected household. The identification information is obtained from the sample household listing and will be given\nby a supervisor.\nThe following are key points in completing the identification section:\n\u2022 Write the name of the place or locality in which the interviewer is working.\n\u2022 Write the name of the head of the household that the interviewer is to interview.\n\u2022 Record the Cluster number and Household number in the boxes to the right of those lines.\nThe rest of the cover page will be filled out after the interviewer conducted the interview.",
   * "published":1,
   * "weight":1009,
   * "created":"",
   * "changed":""},
*/
    public class MySqlSections
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description{ get; set; }
        public string? Notes{ get; set; }
        public int? Published { get; set; }
        public int? Weight { get; set; }
    }

    /*
    * [{"id":1,
    * "quest_group_id":346,
    * "quest_module_id":1009,
    * "weight":1},
    */
    public class MySqlSectionToQuestionnaires
    {
        public int Id { get; set; }
        public int Quest_group_id { get; set; }
        public int Quest_module_id { get; set; }
    }
}
