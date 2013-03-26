using System;
using Main.Core.Documents;
using Newtonsoft.Json;

namespace AndroidMain.Core.Tests.CommonTests
{
	public class StoredEvent
	{
		public string Id { get; set; }
		public long EventSequence { get; set; }
		public Guid EventSourceId { get; set; }
		public Guid CommitId { get; set; }
		public Guid EventIdentifier { get; set; }
		public DateTime EventTimeStamp { get; set; }
		public Version Version { get; set; }
		[JsonProperty(TypeNameHandling = TypeNameHandling.All)]
		public object Data { get; set; }

		#region GetEventText
		private static string GetSerializedEvent()
		{
			return @"
{
  ""EventSequence"": 1,
  ""EventSourceId"": ""2213d3cb-bf96-4c5f-813d-438759066c55"",
  ""CommitId"": ""f9aeb382-367c-49f4-9c74-0ff849e99d1c"",
  ""EventIdentifier"": ""6de866ac-2a8d-4dc9-8a85-bf425b318caa"",
  ""EventTimeStamp"": ""2012-06-28T19:16:14.4760000Z"",
  ""Version"": {
    ""Major"": 0,
    ""Minor"": 0,
    ""Build"": -1,
    ""Revision"": -1,
    ""MajorRevision"": -1,
    ""MinorRevision"": -1
  },
  ""Data"": {
    ""$type"": ""Ncqrs.Restoring.EventStapshoot.SnapshootLoaded, Ncqrs.Restoring.EventStapshoot"",
    ""Template"": {
      ""EventSourceId"": ""2213d3cb-bf96-4c5f-813d-438759066c55"",
      ""Version"": 1,
      ""Payload"": {
        ""$type"": ""Main.Core.Documents.QuestionnaireDocument, Main.Core"",
        ""Children"": [
          {
            ""$type"": ""Main.Core.Entities.SubEntities.Group, Main.Core"",
            ""Children"": [
              {
                ""$type"": ""Main.Core.Entities.SubEntities.Question.SingleQuestion, Main.Core"",
                ""AddSingleAttr"": null,
                ""Children"": [
                  {
                    ""$type"": ""Main.Core.Entities.SubEntities.Answer, Main.Core"",
                    ""AnswerImage"": """",
                    ""AnswerText"": ""Yes"",
                    ""AnswerType"": ""Select"",
                    ""AnswerValue"": ""1"",
                    ""Children"": [],
                    ""Mandatory"": false,
                    ""NameCollection"": null,
                    ""PublicKey"": ""13f206a2-b762-4877-9210-00984883e6c8""
                  },
                  {
                    ""$type"": ""Main.Core.Entities.SubEntities.Answer, Main.Core"",
                    ""AnswerImage"": """",
                    ""AnswerText"": ""No"",
                    ""AnswerType"": ""Select"",
                    ""AnswerValue"": ""0"",
                    ""Children"": [],
                    ""Mandatory"": false,
                    ""NameCollection"": null,
                    ""PublicKey"": ""65e574e2-72b0-4bcf-8f33-6bdd7f8ee916""
                  }
                ],
                ""AnswerOrder"": ""AsIs"",
                ""Capital"": false,
                ""Cards"": [],
                ""Comments"": null,
                ""ConditionExpression"": null,
                ""Featured"": false,
                ""Instructions"": null,
                ""Mandatory"": false,
                ""PublicKey"": ""77362eae-c572-4680-bf7c-773246199e3c"",
                ""QuestionText"": ""Do you khow this man? <img src=\""../../Content/img/bill.jpg\""/>"",
                ""QuestionType"": ""SingleOption"",
                ""StataExportCaption"": null,
                ""ValidationExpression"": null,
                ""ValidationMessage"": null,
                ""Triggers"": null
              },
              {
                ""$type"": ""Main.Core.Entities.SubEntities.Question.SingleQuestion, Main.Core"",
                ""AddSingleAttr"": null,
                ""Children"": [
                  {
                    ""$type"": ""Main.Core.Entities.SubEntities.Answer, Main.Core"",
                    ""AnswerImage"": null,
                    ""AnswerText"": ""12-18"",
                    ""AnswerType"": ""Select"",
                    ""AnswerValue"": null,
                    ""Children"": [],
                    ""Mandatory"": false,
                    ""NameCollection"": null,
                    ""PublicKey"": ""c1cd8569-eef5-43f3-a7bb-4d8fdf171f03""
                  },
                  {
                    ""$type"": ""Main.Core.Entities.SubEntities.Answer, Main.Core"",
                    ""AnswerImage"": null,
                    ""AnswerText"": ""19-23"",
                    ""AnswerType"": ""Select"",
                    ""AnswerValue"": null,
                    ""Children"": [],
                    ""Mandatory"": false,
                    ""NameCollection"": null,
                    ""PublicKey"": ""8e8f4b72-ed75-4f70-b1f5-7892d3094a3a""
                  },
                  {
                    ""$type"": ""Main.Core.Entities.SubEntities.Answer, Main.Core"",
                    ""AnswerImage"": null,
                    ""AnswerText"": ""24-35"",
                    ""AnswerType"": ""Select"",
                    ""AnswerValue"": null,
                    ""Children"": [],
                    ""Mandatory"": false,
                    ""NameCollection"": null,
                    ""PublicKey"": ""94f915c8-096b-4b25-8a15-36d8a235c982""
                  },
                  {
                    ""$type"": ""Main.Core.Entities.SubEntities.Answer, Main.Core"",
                    ""AnswerImage"": null,
                    ""AnswerText"": ""36-..."",
                    ""AnswerType"": ""Select"",
                    ""AnswerValue"": null,
                    ""Children"": [],
                    ""Mandatory"": false,
                    ""NameCollection"": null,
                    ""PublicKey"": ""ace01169-eea0-4cd4-9ced-eabdf5acc0f4""
                  }
                ],
                ""AnswerOrder"": ""AsIs"",
                ""Capital"": false,
                ""Cards"": [],
                ""Comments"": null,
                ""ConditionExpression"": null,
                ""Featured"": true,
                ""Instructions"": null,
                ""Mandatory"": false,
                ""PublicKey"": ""011e4778-4f21-4f60-b4b3-353fb977ec27"",
                ""QuestionText"": ""Respondent's age:"",
                ""QuestionType"": ""SingleOption"",
                ""StataExportCaption"": null,
                ""ValidationExpression"": null,
                ""ValidationMessage"": null,
                ""Triggers"": null
              },
              {
                ""$type"": ""Main.Core.Entities.SubEntities.Question.SingleQuestion, Main.Core"",
                ""AddSingleAttr"": null,
                ""Children"": [
                  {
                    ""$type"": ""Main.Core.Entities.SubEntities.Answer, Main.Core"",
                    ""AnswerImage"": null,
                    ""AnswerText"": ""Illiterate"",
                    ""AnswerType"": ""Select"",
                    ""AnswerValue"": null,
                    ""Children"": [],
                    ""Mandatory"": false,
                    ""NameCollection"": null,
                    ""PublicKey"": ""53a8511b-37ba-40f6-86f3-ec6dc8efea44""
                  },
                  {
                    ""$type"": ""Main.Core.Entities.SubEntities.Answer, Main.Core"",
                    ""AnswerImage"": null,
                    ""AnswerText"": ""Primary"",
                    ""AnswerType"": ""Select"",
                    ""AnswerValue"": null,
                    ""Children"": [],
                    ""Mandatory"": false,
                    ""NameCollection"": null,
                    ""PublicKey"": ""cad0ecfd-b5cd-43d4-a0bb-d59be07a0e78""
                  },
                  {
                    ""$type"": ""Main.Core.Entities.SubEntities.Answer, Main.Core"",
                    ""AnswerImage"": null,
                    ""AnswerText"": ""Secondary"",
                    ""AnswerType"": ""Select"",
                    ""AnswerValue"": null,
                    ""Children"": [],
                    ""Mandatory"": false,
                    ""NameCollection"": null,
                    ""PublicKey"": ""beb7d33a-1ffa-4f04-b3eb-0e0ee623e710""
                  },
                  {
                    ""$type"": ""Main.Core.Entities.SubEntities.Answer, Main.Core"",
                    ""AnswerImage"": null,
                    ""AnswerText"": ""High school diploma"",
                    ""AnswerType"": ""Select"",
                    ""AnswerValue"": null,
                    ""Children"": [],
                    ""Mandatory"": false,
                    ""NameCollection"": null,
                    ""PublicKey"": ""ab1a577c-1c37-4a61-b76a-b3c528ff7e61""
                  },
                  {
                    ""$type"": ""Main.Core.Entities.SubEntities.Answer, Main.Core"",
                    ""AnswerImage"": null,
                    ""AnswerText"": ""University degree"",
                    ""AnswerType"": ""Select"",
                    ""AnswerValue"": null,
                    ""Children"": [],
                    ""Mandatory"": false,
                    ""NameCollection"": null,
                    ""PublicKey"": ""4c65ce9f-4a86-4070-be05-531e2cad8ac3""
                  },
                  {
                    ""$type"": ""Main.Core.Entities.SubEntities.Answer, Main.Core"",
                    ""AnswerImage"": null,
                    ""AnswerText"": ""Masters"",
                    ""AnswerType"": ""Select"",
                    ""AnswerValue"": null,
                    ""Children"": [],
                    ""Mandatory"": false,
                    ""NameCollection"": null,
                    ""PublicKey"": ""905699cc-d66e-44c5-ab31-137174ec3791""
                  },
                  {
                    ""$type"": ""Main.Core.Entities.SubEntities.Answer, Main.Core"",
                    ""AnswerImage"": null,
                    ""AnswerText"": ""Ph.D."",
                    ""AnswerType"": ""Select"",
                    ""AnswerValue"": null,
                    ""Children"": [],
                    ""Mandatory"": false,
                    ""NameCollection"": null,
                    ""PublicKey"": ""344d6d33-9f5e-415d-b973-d0f2fe226c65""
                  }
                ],
                ""AnswerOrder"": ""Random"",
                ""Capital"": false,
                ""Cards"": [],
                ""Comments"": null,
                ""ConditionExpression"": null,
                ""Featured"": true,
                ""Instructions"": null,
                ""Mandatory"": false,
                ""PublicKey"": ""750e0019-047a-4fbd-a69d-8821ee6ba12b"",
                ""QuestionText"": ""Respondent's highest level of education:"",
                ""QuestionType"": ""SingleOption"",
                ""StataExportCaption"": null,
                ""ValidationExpression"": null,
                ""ValidationMessage"": null,
                ""Triggers"": null
              },
              {
                ""$type"": ""Main.Core.Entities.SubEntities.Question.TextQuestion, Main.Core"",
                ""AddTextAttr"": null,
                ""Children"": [],
                ""AnswerOrder"": ""AsIs"",
                ""Capital"": false,
                ""Cards"": [],
                ""Comments"": null,
                ""ConditionExpression"": null,
                ""Featured"": true,
                ""Instructions"": null,
                ""Mandatory"": false,
                ""PublicKey"": ""3d9f69ac-1670-4ace-9715-30dcea62fd2e"",
                ""QuestionText"": ""Respondent's name of school"",
                ""QuestionType"": ""Text"",
                ""StataExportCaption"": null,
                ""ValidationExpression"": null,
                ""ValidationMessage"": null,
                ""Triggers"": null
              }
            ],
            ""ConditionExpression"": """",
            ""Enabled"": false,
            ""Description"": """",
            ""Propagated"": ""None"",
            ""PublicKey"": ""53279073-5b77-4ab8-954c-6f949430cc80"",
            ""Title"": ""GENERAL"",
            ""Triggers"": []
          },
          {
            ""$type"": ""Main.Core.Entities.SubEntities.Group, Main.Core"",
            ""Children"": [
              {
                ""$type"": ""Main.Core.Entities.SubEntities.Question.SingleQuestion, Main.Core"",
                ""AddSingleAttr"": null,
                ""Children"": [
                  {
                    ""$type"": ""Main.Core.Entities.SubEntities.Answer, Main.Core"",
                    ""AnswerImage"": """",
                    ""AnswerText"": ""Yes"",
                    ""AnswerType"": ""Select"",
                    ""AnswerValue"": ""1"",
                    ""Children"": [],
                    ""Mandatory"": false,
                    ""NameCollection"": null,
                    ""PublicKey"": ""7ca88193-aec2-4315-8647-9db365e122d9""
                  },
                  {
                    ""$type"": ""Main.Core.Entities.SubEntities.Answer, Main.Core"",
                    ""AnswerImage"": """",
                    ""AnswerText"": ""No"",
                    ""AnswerType"": ""Select"",
                    ""AnswerValue"": ""0"",
                    ""Children"": [],
                    ""Mandatory"": false,
                    ""NameCollection"": null,
                    ""PublicKey"": ""47853bdf-420e-476c-adcb-094f8849f001""
                  }
                ],
                ""AnswerOrder"": ""AsIs"",
                ""Capital"": false,
                ""Cards"": [],
                ""Comments"": null,
                ""ConditionExpression"": null,
                ""Featured"": false,
                ""Instructions"": null,
                ""Mandatory"": false,
                ""PublicKey"": ""4d1405ad-352a-4116-a26a-47e824fbb4cf"",
                ""QuestionText"": ""Do you have a pet?"",
                ""QuestionType"": ""YesNo"",
                ""StataExportCaption"": null,
                ""ValidationExpression"": null,
                ""ValidationMessage"": null,
                ""Triggers"": null
              },
              {
                ""$type"": ""Main.Core.Entities.SubEntities.Question.DateTimeQuestion, Main.Core"",
                ""AddDateTimeAttr"": null,
                ""Children"": [],
                ""DateTimeAttr"": ""0001-01-01T07:00:00.0000000"",
                ""AnswerOrder"": ""AsIs"",
                ""Capital"": false,
                ""Cards"": [],
                ""Comments"": null,
                ""ConditionExpression"": ""[4d1405ad-352a-4116-a26a-47e824fbb4cf]=1"",
                ""Featured"": false,
                ""Instructions"": null,
                ""Mandatory"": false,
                ""PublicKey"": ""18f3b435-60b5-4a69-bbf5-d915809efc2b"",
                ""QuestionText"": ""What is it date of birth"",
                ""QuestionType"": ""DateTime"",
                ""StataExportCaption"": null,
                ""ValidationExpression"": null,
                ""ValidationMessage"": null,
                ""Triggers"": null
              },
              {
                ""$type"": ""Main.Core.Entities.SubEntities.Question.SingleQuestion, Main.Core"",
                ""AddSingleAttr"": null,
                ""Children"": [
                  {
                    ""$type"": ""Main.Core.Entities.SubEntities.Answer, Main.Core"",
                    ""AnswerImage"": """",
                    ""AnswerText"": ""Yes"",
                    ""AnswerType"": ""Select"",
                    ""AnswerValue"": ""1"",
                    ""Children"": [],
                    ""Mandatory"": false,
                    ""NameCollection"": null,
                    ""PublicKey"": ""89c68eb6-c3f5-49d9-94f9-53b7fff6f9cc""
                  },
                  {
                    ""$type"": ""Main.Core.Entities.SubEntities.Answer, Main.Core"",
                    ""AnswerImage"": """",
                    ""AnswerText"": ""No"",
                    ""AnswerType"": ""Select"",
                    ""AnswerValue"": ""0"",
                    ""Children"": [],
                    ""Mandatory"": false,
                    ""NameCollection"": null,
                    ""PublicKey"": ""a9c00784-f6b3-4faf-ab32-468c4f22ac74""
                  }
                ],
                ""AnswerOrder"": ""AsIs"",
                ""Capital"": false,
                ""Cards"": [],
                ""Comments"": null,
                ""ConditionExpression"": ""[4d1405ad-352a-4116-a26a-47e824fbb4cf]=1"",
                ""Featured"": false,
                ""Instructions"": null,
                ""Mandatory"": false,
                ""PublicKey"": ""90f3cf5a-0fd1-4e33-8a2c-36365e905752"",
                ""QuestionText"": ""Does it live with you?"",
                ""QuestionType"": ""YesNo"",
                ""StataExportCaption"": null,
                ""ValidationExpression"": null,
                ""ValidationMessage"": null,
                ""Triggers"": null
              },
              {
                ""$type"": ""Main.Core.Entities.SubEntities.Question.NumericQuestion, Main.Core"",
                ""AddNumericAttr"": null,
                ""Children"": [],
                ""IntAttr"": 0,
                ""AnswerOrder"": ""AsIs"",
                ""Capital"": false,
                ""Cards"": [],
                ""Comments"": null,
                ""ConditionExpression"": ""[4d1405ad-352a-4116-a26a-47e824fbb4cf]=1&&[90f3cf5a-0fd1-4e33-8a2c-36365e905752]=1"",
                ""Featured"": false,
                ""Instructions"": null,
                ""Mandatory"": false,
                ""PublicKey"": ""3482cee6-548d-41c3-a024-db94fedc5873"",
                ""QuestionText"": ""How many times per day do you walk your pet?"",
                ""QuestionType"": ""Numeric"",
                ""StataExportCaption"": null,
                ""ValidationExpression"": null,
                ""ValidationMessage"": null,
                ""Triggers"": null
              },
              {
                ""$type"": ""Main.Core.Entities.SubEntities.Question.TextQuestion, Main.Core"",
                ""AddTextAttr"": null,
                ""Children"": [],
                ""AnswerOrder"": ""AsIs"",
                ""Capital"": false,
                ""Cards"": [],
                ""Comments"": null,
                ""ConditionExpression"": ""[4d1405ad-352a-4116-a26a-47e824fbb4cf]=1"",
                ""Featured"": false,
                ""Instructions"": null,
                ""Mandatory"": false,
                ""PublicKey"": ""20820feb-ccd4-4049-8bc6-2018e222b4f1"",
                ""QuestionText"": ""What is your pet name?"",
                ""QuestionType"": ""Text"",
                ""StataExportCaption"": null,
                ""ValidationExpression"": null,
                ""ValidationMessage"": null,
                ""Triggers"": null
              }
            ],
            ""ConditionExpression"": """",
            ""Enabled"": false,
            ""Description"": """",
            ""Propagated"": ""None"",
            ""PublicKey"": ""1068cac2-8fd2-43bb-bc57-04607364152e"",
            ""Title"": ""PETS"",
            ""Triggers"": []
          },
          {
            ""$type"": ""Main.Core.Entities.SubEntities.Group, Main.Core"",
            ""Children"": [
              {
                ""$type"": ""Main.Core.Entities.SubEntities.Question.SingleQuestion, Main.Core"",
                ""AddSingleAttr"": null,
                ""Children"": [
                  {
                    ""$type"": ""Main.Core.Entities.SubEntities.Answer, Main.Core"",
                    ""AnswerImage"": """",
                    ""AnswerText"": ""Yes"",
                    ""AnswerType"": ""Select"",
                    ""AnswerValue"": ""1"",
                    ""Children"": [],
                    ""Mandatory"": false,
                    ""NameCollection"": null,
                    ""PublicKey"": ""af008918-3308-4730-b6b7-13bf8a94e571""
                  },
                  {
                    ""$type"": ""Main.Core.Entities.SubEntities.Answer, Main.Core"",
                    ""AnswerImage"": """",
                    ""AnswerText"": ""No"",
                    ""AnswerType"": ""Select"",
                    ""AnswerValue"": ""0"",
                    ""Children"": [],
                    ""Mandatory"": false,
                    ""NameCollection"": null,
                    ""PublicKey"": ""bd6b2ebd-d485-4f95-a287-d79a66aa2e5f""
                  }
                ],
                ""AnswerOrder"": ""AsIs"",
                ""Capital"": false,
                ""Cards"": [],
                ""Comments"": null,
                ""ConditionExpression"": null,
                ""Featured"": false,
                ""Instructions"": null,
                ""Mandatory"": false,
                ""PublicKey"": ""879df044-c8ca-442f-a0d8-aa967c53281e"",
                ""QuestionText"": ""Do you like the World Bank cafeteria?"",
                ""QuestionType"": ""YesNo"",
                ""StataExportCaption"": null,
                ""ValidationExpression"": null,
                ""ValidationMessage"": null,
                ""Triggers"": null
              },
              {
                ""$type"": ""Main.Core.Entities.SubEntities.Question.SingleQuestion, Main.Core"",
                ""AddSingleAttr"": null,
                ""Children"": [
                  {
                    ""$type"": ""Main.Core.Entities.SubEntities.Answer, Main.Core"",
                    ""AnswerImage"": """",
                    ""AnswerText"": ""a. Service"",
                    ""AnswerType"": ""Select"",
                    ""AnswerValue"": null,
                    ""Children"": [],
                    ""Mandatory"": false,
                    ""NameCollection"": null,
                    ""PublicKey"": ""5f6664e7-5797-4bae-a3ed-0b8f6ed7fb52""
                  },
                  {
                    ""$type"": ""Main.Core.Entities.SubEntities.Answer, Main.Core"",
                    ""AnswerImage"": """",
                    ""AnswerText"": ""b. Menu"",
                    ""AnswerType"": ""Select"",
                    ""AnswerValue"": null,
                    ""Children"": [],
                    ""Mandatory"": false,
                    ""NameCollection"": null,
                    ""PublicKey"": ""ed965d26-5ea2-479d-8ebe-128574f275f6""
                  },
                  {
                    ""$type"": ""Main.Core.Entities.SubEntities.Answer, Main.Core"",
                    ""AnswerImage"": """",
                    ""AnswerText"": ""c. Prices"",
                    ""AnswerType"": ""Select"",
                    ""AnswerValue"": null,
                    ""Children"": [],
                    ""Mandatory"": false,
                    ""NameCollection"": null,
                    ""PublicKey"": ""02a7931d-887d-4c9a-b3a3-01f6e3b49bfe""
                  },
                  {
                    ""$type"": ""Main.Core.Entities.SubEntities.Answer, Main.Core"",
                    ""AnswerImage"": """",
                    ""AnswerText"": ""d. Location"",
                    ""AnswerType"": ""Select"",
                    ""AnswerValue"": null,
                    ""Children"": [],
                    ""Mandatory"": false,
                    ""NameCollection"": null,
                    ""PublicKey"": ""b76dc690-86b7-4f9c-ac6a-e5e67c750e09""
                  }
                ],
                ""AnswerOrder"": ""AsIs"",
                ""Capital"": false,
                ""Cards"": [],
                ""Comments"": null,
                ""ConditionExpression"": null,
                ""Featured"": false,
                ""Instructions"": null,
                ""Mandatory"": false,
                ""PublicKey"": ""a3ce15ab-0745-4ee6-bb19-7e91f691c75a"",
                ""QuestionText"": ""What is the best station in the cafeteria?"",
                ""QuestionType"": ""SingleOption"",
                ""StataExportCaption"": null,
                ""ValidationExpression"": null,
                ""ValidationMessage"": null,
                ""Triggers"": null
              },
              {
                ""$type"": ""Main.Core.Entities.SubEntities.Question.SingleQuestion, Main.Core"",
                ""AddSingleAttr"": null,
                ""Children"": [
                  {
                    ""$type"": ""Main.Core.Entities.SubEntities.Answer, Main.Core"",
                    ""AnswerImage"": """",
                    ""AnswerText"": ""a. Service"",
                    ""AnswerType"": ""Select"",
                    ""AnswerValue"": null,
                    ""Children"": [],
                    ""Mandatory"": false,
                    ""NameCollection"": null,
                    ""PublicKey"": ""8a7c32fd-fea7-4fd3-b956-2e8dea42fbc9""
                  },
                  {
                    ""$type"": ""Main.Core.Entities.SubEntities.Answer, Main.Core"",
                    ""AnswerImage"": """",
                    ""AnswerText"": ""b. Menu"",
                    ""AnswerType"": ""Select"",
                    ""AnswerValue"": null,
                    ""Children"": [],
                    ""Mandatory"": false,
                    ""NameCollection"": null,
                    ""PublicKey"": ""bd6e4785-f5d8-4306-afb6-62e5ea72f316""
                  },
                  {
                    ""$type"": ""Main.Core.Entities.SubEntities.Answer, Main.Core"",
                    ""AnswerImage"": """",
                    ""AnswerText"": ""c. Prices"",
                    ""AnswerType"": ""Select"",
                    ""AnswerValue"": null,
                    ""Children"": [],
                    ""Mandatory"": false,
                    ""NameCollection"": null,
                    ""PublicKey"": ""9675854e-ba8f-4e82-aa1a-86eaa72ce54d""
                  },
                  {
                    ""$type"": ""Main.Core.Entities.SubEntities.Answer, Main.Core"",
                    ""AnswerImage"": """",
                    ""AnswerText"": ""d. Location"",
                    ""AnswerType"": ""Select"",
                    ""AnswerValue"": null,
                    ""Children"": [],
                    ""Mandatory"": false,
                    ""NameCollection"": null,
                    ""PublicKey"": ""3e6aab03-0c7d-4b43-baa8-c2a17bcd467a""
                  }
                ],
                ""AnswerOrder"": ""AsIs"",
                ""Capital"": false,
                ""Cards"": [],
                ""Comments"": null,
                ""ConditionExpression"": null,
                ""Featured"": false,
                ""Instructions"": null,
                ""Mandatory"": false,
                ""PublicKey"": ""b6a042fc-6f87-4d87-8245-d442e2f40347"",
                ""QuestionText"": ""What is the worst station in the cafeteria?"",
                ""QuestionType"": ""SingleOption"",
                ""StataExportCaption"": null,
                ""ValidationExpression"": ""[a3ce15ab-0745-4ee6-bb19-7e91f691c75a]!=[b6a042fc-6f87-4d87-8245-d442e2f40347]"",
                ""ValidationMessage"": ""Are you shure? Like and dislike cannot be the same!"",
                ""Triggers"": null
              }
            ],
            ""ConditionExpression"": """",
            ""Enabled"": false,
            ""Description"": """",
            ""Propagated"": ""None"",
            ""PublicKey"": ""e2aaeffe-4b4c-4db1-a773-83bebc394543"",
            ""Title"": ""CAFETERIA"",
            ""Triggers"": []
          },
          {
            ""$type"": ""Main.Core.Entities.SubEntities.Group, Main.Core"",
            ""Children"": [
              {
                ""$type"": ""Main.Core.Entities.SubEntities.Question.SingleQuestion, Main.Core"",
                ""AddSingleAttr"": null,
                ""Children"": [
                  {
                    ""$type"": ""Main.Core.Entities.SubEntities.Answer, Main.Core"",
                    ""AnswerImage"": """",
                    ""AnswerText"": ""Yes"",
                    ""AnswerType"": ""Select"",
                    ""AnswerValue"": ""1"",
                    ""Children"": [],
                    ""Mandatory"": false,
                    ""NameCollection"": null,
                    ""PublicKey"": ""590bc175-927f-4687-91d1-9eaab73a7f6f""
                  },
                  {
                    ""$type"": ""Main.Core.Entities.SubEntities.Answer, Main.Core"",
                    ""AnswerImage"": """",
                    ""AnswerText"": ""No"",
                    ""AnswerType"": ""Select"",
                    ""AnswerValue"": ""0"",
                    ""Children"": [],
                    ""Mandatory"": false,
                    ""NameCollection"": null,
                    ""PublicKey"": ""d0e08f4d-72fd-407e-a86e-96ad7b2b0463""
                  }
                ],
                ""AnswerOrder"": ""AsIs"",
                ""Capital"": false,
                ""Cards"": [],
                ""Comments"": null,
                ""ConditionExpression"": null,
                ""Featured"": false,
                ""Instructions"": ""The Millennium Cafe offers many delightful treats, salads, sandwiches, and signature drinks."",
                ""Mandatory"": false,
                ""PublicKey"": ""b81ec277-375d-458f-b88d-cfc6af8f20e5"",
                ""QuestionText"": ""Do you visit millennium cafe?"",
                ""QuestionType"": ""YesNo"",
                ""StataExportCaption"": null,
                ""ValidationExpression"": null,
                ""ValidationMessage"": null,
                ""Triggers"": null
              },
              {
                ""$type"": ""Main.Core.Entities.SubEntities.Question.MultyOptionsQuestion, Main.Core"",
                ""AddMultyAttr"": null,
                ""Children"": [
                  {
                    ""$type"": ""Main.Core.Entities.SubEntities.Answer, Main.Core"",
                    ""AnswerImage"": """",
                    ""AnswerText"": ""a. Tea"",
                    ""AnswerType"": ""Select"",
                    ""AnswerValue"": null,
                    ""Children"": [],
                    ""Mandatory"": false,
                    ""NameCollection"": null,
                    ""PublicKey"": ""74e12c58-9030-4de6-b97e-faf50bc181ec""
                  },
                  {
                    ""$type"": ""Main.Core.Entities.SubEntities.Answer, Main.Core"",
                    ""AnswerImage"": """",
                    ""AnswerText"": ""b. Coffee"",
                    ""AnswerType"": ""Select"",
                    ""AnswerValue"": null,
                    ""Children"": [],
                    ""Mandatory"": false,
                    ""NameCollection"": null,
                    ""PublicKey"": ""6632d6f1-ff7a-49db-abe3-661d64fcdda4""
                  },
                  {
                    ""$type"": ""Main.Core.Entities.SubEntities.Answer, Main.Core"",
                    ""AnswerImage"": """",
                    ""AnswerText"": ""c. Milk"",
                    ""AnswerType"": ""Select"",
                    ""AnswerValue"": null,
                    ""Children"": [],
                    ""Mandatory"": false,
                    ""NameCollection"": null,
                    ""PublicKey"": ""edd86f1e-68c5-44bc-9cb2-1056eb8f022e""
                  },
                  {
                    ""$type"": ""Main.Core.Entities.SubEntities.Answer, Main.Core"",
                    ""AnswerImage"": """",
                    ""AnswerText"": ""d. Soy Milk"",
                    ""AnswerType"": ""Select"",
                    ""AnswerValue"": null,
                    ""Children"": [],
                    ""Mandatory"": false,
                    ""NameCollection"": null,
                    ""PublicKey"": ""fe3bf0d6-906b-4ae7-8716-b246f387345a""
                  },
                  {
                    ""$type"": ""Main.Core.Entities.SubEntities.Answer, Main.Core"",
                    ""AnswerImage"": """",
                    ""AnswerText"": ""e. Carbonated drinks"",
                    ""AnswerType"": ""Select"",
                    ""AnswerValue"": null,
                    ""Children"": [],
                    ""Mandatory"": false,
                    ""NameCollection"": null,
                    ""PublicKey"": ""ca73893e-87a8-4936-8b82-a0df98e0516a""
                  }
                ],
                ""AnswerOrder"": ""AsIs"",
                ""Capital"": false,
                ""Cards"": [],
                ""Comments"": null,
                ""ConditionExpression"": ""[b81ec277-375d-458f-b88d-cfc6af8f20e5]=1"",
                ""Featured"": false,
                ""Instructions"": null,
                ""Mandatory"": false,
                ""PublicKey"": ""65972bff-9a2a-4667-9932-71440182a2ba"",
                ""QuestionText"": ""What do you order there?"",
                ""QuestionType"": ""MultyOption"",
                ""StataExportCaption"": null,
                ""ValidationExpression"": null,
                ""ValidationMessage"": null,
                ""Triggers"": null
              }
            ],
            ""ConditionExpression"": """",
            ""Enabled"": false,
            ""Description"": """",
            ""Propagated"": ""None"",
            ""PublicKey"": ""2c2706ff-3376-41d3-b0bf-53d0a25df178"",
            ""Title"": ""MILLENNIUM CAFE"",
            ""Triggers"": []
          },
          {
            ""$type"": ""Main.Core.Entities.SubEntities.Group, Main.Core"",
            ""Children"": [
              {
                ""$type"": ""Main.Core.Entities.SubEntities.Question.MultyOptionsQuestion, Main.Core"",
                ""AddMultyAttr"": null,
                ""Children"": [
                  {
                    ""$type"": ""Main.Core.Entities.SubEntities.Answer, Main.Core"",
                    ""AnswerImage"": """",
                    ""AnswerText"": ""a. Bus"",
                    ""AnswerType"": ""Select"",
                    ""AnswerValue"": null,
                    ""Children"": [],
                    ""Mandatory"": false,
                    ""NameCollection"": null,
                    ""PublicKey"": ""d6c3387d-ffbb-4764-877c-0fb437956e71""
                  },
                  {
                    ""$type"": ""Main.Core.Entities.SubEntities.Answer, Main.Core"",
                    ""AnswerImage"": """",
                    ""AnswerText"": ""b. Metro"",
                    ""AnswerType"": ""Select"",
                    ""AnswerValue"": null,
                    ""Children"": [],
                    ""Mandatory"": false,
                    ""NameCollection"": null,
                    ""PublicKey"": ""4f07e489-a972-4a89-a786-62474f08e8c0""
                  },
                  {
                    ""$type"": ""Main.Core.Entities.SubEntities.Answer, Main.Core"",
                    ""AnswerImage"": """",
                    ""AnswerText"": ""c. Walk"",
                    ""AnswerType"": ""Select"",
                    ""AnswerValue"": null,
                    ""Children"": [],
                    ""Mandatory"": false,
                    ""NameCollection"": null,
                    ""PublicKey"": ""5170744e-f62d-4672-8aed-82d72db5d87d""
                  },
                  {
                    ""$type"": ""Main.Core.Entities.SubEntities.Answer, Main.Core"",
                    ""AnswerImage"": """",
                    ""AnswerText"": ""d. Car"",
                    ""AnswerType"": ""Select"",
                    ""AnswerValue"": null,
                    ""Children"": [],
                    ""Mandatory"": false,
                    ""NameCollection"": null,
                    ""PublicKey"": ""bdd54223-4e8f-49d0-8e60-afe5c5e786f3""
                  },
                  {
                    ""$type"": ""Main.Core.Entities.SubEntities.Answer, Main.Core"",
                    ""AnswerImage"": """",
                    ""AnswerText"": ""e. Bike"",
                    ""AnswerType"": ""Select"",
                    ""AnswerValue"": null,
                    ""Children"": [],
                    ""Mandatory"": false,
                    ""NameCollection"": null,
                    ""PublicKey"": ""c29b9260-7ae5-46b1-9506-a21b13e7dc6a""
                  }
                ],
                ""AnswerOrder"": ""AsIs"",
                ""Capital"": false,
                ""Cards"": [],
                ""Comments"": null,
                ""ConditionExpression"": null,
                ""Featured"": false,
                ""Instructions"": null,
                ""Mandatory"": true,
                ""PublicKey"": ""c6f45d88-db63-4461-a8a3-07dea5f6849c"",
                ""QuestionText"": ""How do you go to work?"",
                ""QuestionType"": ""MultyOption"",
                ""StataExportCaption"": null,
                ""ValidationExpression"": null,
                ""ValidationMessage"": null,
                ""Triggers"": null
              }
            ],
            ""ConditionExpression"": """",
            ""Enabled"": false,
            ""Description"": """",
            ""Propagated"": ""None"",
            ""PublicKey"": ""550226d2-759a-473f-b026-dc6693527819"",
            ""Title"": ""TRANSPORT"",
            ""Triggers"": []
          },
          {
            ""$type"": ""Main.Core.Entities.SubEntities.Group, Main.Core"",
            ""Children"": [
              {
                ""$type"": ""Main.Core.Entities.SubEntities.Question.SingleQuestion, Main.Core"",
                ""AddSingleAttr"": null,
                ""Children"": [
                  {
                    ""$type"": ""Main.Core.Entities.SubEntities.Answer, Main.Core"",
                    ""AnswerImage"": """",
                    ""AnswerText"": ""Yes"",
                    ""AnswerType"": ""Select"",
                    ""AnswerValue"": ""1"",
                    ""Children"": [],
                    ""Mandatory"": false,
                    ""NameCollection"": null,
                    ""PublicKey"": ""e42207c3-1bcd-4de6-8fd1-4ad9b6d9991f""
                  },
                  {
                    ""$type"": ""Main.Core.Entities.SubEntities.Answer, Main.Core"",
                    ""AnswerImage"": """",
                    ""AnswerText"": ""No"",
                    ""AnswerType"": ""Select"",
                    ""AnswerValue"": ""0"",
                    ""Children"": [],
                    ""Mandatory"": false,
                    ""NameCollection"": null,
                    ""PublicKey"": ""147f3ff3-f813-4c8a-89ab-33610e946fa6""
                  }
                ],
                ""AnswerOrder"": ""AsIs"",
                ""Capital"": false,
                ""Cards"": [],
                ""Comments"": null,
                ""ConditionExpression"": null,
                ""Featured"": false,
                ""Instructions"": null,
                ""Mandatory"": false,
                ""PublicKey"": ""8c17f588-9e48-4115-bf52-474b6f429b02"",
                ""QuestionText"": ""Do you own any Apple products?"",
                ""QuestionType"": ""YesNo"",
                ""StataExportCaption"": null,
                ""ValidationExpression"": null,
                ""ValidationMessage"": null,
                ""Triggers"": null
              },
              {
                ""$type"": ""Main.Core.Entities.SubEntities.Question.MultyOptionsQuestion, Main.Core"",
                ""AddMultyAttr"": null,
                ""Children"": [
                  {
                    ""$type"": ""Main.Core.Entities.SubEntities.Answer, Main.Core"",
                    ""AnswerImage"": ""45ccb0e6-fc33-4575-b961-7a309c1b28e7"",
                    ""AnswerText"": ""a. IMac "",
                    ""AnswerType"": ""Image"",
                    ""AnswerValue"": null,
                    ""Children"": [],
                    ""Mandatory"": false,
                    ""NameCollection"": null,
                    ""PublicKey"": ""00c7906a-0e6c-4149-bea3-737e69677d1a""
                  },
                  {
                    ""$type"": ""Main.Core.Entities.SubEntities.Answer, Main.Core"",
                    ""AnswerImage"": ""5ac803f5-6983-4b3e-8f92-0e8730daa62c"",
                    ""AnswerText"": ""b. IPAD"",
                    ""AnswerType"": ""Image"",
                    ""AnswerValue"": null,
                    ""Children"": [],
                    ""Mandatory"": false,
                    ""NameCollection"": null,
                    ""PublicKey"": ""2ba21ed5-b692-4053-8744-a63c87ef793b""
                  },
                  {
                    ""$type"": ""Main.Core.Entities.SubEntities.Answer, Main.Core"",
                    ""AnswerImage"": ""a9734df3-c1d3-4d01-9644-80539f243022"",
                    ""AnswerText"": ""c. IPOD"",
                    ""AnswerType"": ""Image"",
                    ""AnswerValue"": null,
                    ""Children"": [],
                    ""Mandatory"": false,
                    ""NameCollection"": null,
                    ""PublicKey"": ""6e30fa97-d7e0-4047-95e9-9e0e07ac60ba""
                  },
                  {
                    ""$type"": ""Main.Core.Entities.SubEntities.Answer, Main.Core"",
                    ""AnswerImage"": ""cad897b1-f1a2-472e-9b3a-ef644ddb2545"",
                    ""AnswerText"": ""d. IPHONE"",
                    ""AnswerType"": ""Image"",
                    ""AnswerValue"": null,
                    ""Children"": [],
                    ""Mandatory"": false,
                    ""NameCollection"": null,
                    ""PublicKey"": ""3cef7244-7fce-489b-8d1a-a3bf17713529""
                  }
                ],
                ""AnswerOrder"": ""AsIs"",
                ""Capital"": false,
                ""Cards"": [],
                ""Comments"": null,
                ""ConditionExpression"": ""[8c17f588-9e48-4115-bf52-474b6f429b02]=1"",
                ""Featured"": false,
                ""Instructions"": null,
                ""Mandatory"": false,
                ""PublicKey"": ""e7f79783-209c-4f63-9f80-7b6c63dc9298"",
                ""QuestionText"": ""What are they?"",
                ""QuestionType"": ""MultyOption"",
                ""StataExportCaption"": null,
                ""ValidationExpression"": null,
                ""ValidationMessage"": null,
                ""Triggers"": null
              }
            ],
            ""ConditionExpression"": """",
            ""Enabled"": false,
            ""Description"": """",
            ""Propagated"": ""None"",
            ""PublicKey"": ""e7fb1f6c-cbe5-4083-936c-8ef77d0e832f"",
            ""Title"": ""APPLE PRODUCTS"",
            ""Triggers"": []
          },
          {
            ""$type"": ""Main.Core.Entities.SubEntities.Group, Main.Core"",
            ""Children"": [
              {
                ""$type"": ""Main.Core.Entities.SubEntities.Question.SingleQuestion, Main.Core"",
                ""AddSingleAttr"": null,
                ""Children"": [
                  {
                    ""$type"": ""Main.Core.Entities.SubEntities.Answer, Main.Core"",
                    ""AnswerImage"": """",
                    ""AnswerText"": ""Rent"",
                    ""AnswerType"": ""Select"",
                    ""AnswerValue"": ""1"",
                    ""Children"": [],
                    ""Mandatory"": false,
                    ""NameCollection"": null,
                    ""PublicKey"": ""91ad3cd0-7298-4b29-a4cc-246ed17eca0a""
                  },
                  {
                    ""$type"": ""Main.Core.Entities.SubEntities.Answer, Main.Core"",
                    ""AnswerImage"": """",
                    ""AnswerText"": ""Own"",
                    ""AnswerType"": ""Select"",
                    ""AnswerValue"": ""0"",
                    ""Children"": [],
                    ""Mandatory"": false,
                    ""NameCollection"": null,
                    ""PublicKey"": ""f461410b-66e1-41cd-baf7-192b30baa99b""
                  },
                  {
                    ""$type"": ""Main.Core.Entities.SubEntities.Answer, Main.Core"",
                    ""AnswerImage"": """",
                    ""AnswerText"": ""Other"",
                    ""AnswerType"": ""Select"",
                    ""AnswerValue"": ""2"",
                    ""Children"": [],
                    ""Mandatory"": false,
                    ""NameCollection"": null,
                    ""PublicKey"": ""e2d12972-d59e-4733-8381-1c6ada288cf0""
                  }
                ],
                ""AnswerOrder"": ""AsIs"",
                ""Capital"": false,
                ""Cards"": [],
                ""Comments"": null,
                ""ConditionExpression"": null,
                ""Featured"": false,
                ""Instructions"": null,
                ""Mandatory"": false,
                ""PublicKey"": ""a9a7a6e3-ea53-41e9-8795-a7edb36ddb5d"",
                ""QuestionText"": ""Do you rent or own your house?"",
                ""QuestionType"": ""SingleOption"",
                ""StataExportCaption"": null,
                ""ValidationExpression"": null,
                ""ValidationMessage"": null,
                ""Triggers"": null
              },
              {
                ""$type"": ""Main.Core.Entities.SubEntities.Question.TextQuestion, Main.Core"",
                ""AddTextAttr"": null,
                ""Children"": [],
                ""AnswerOrder"": ""AsIs"",
                ""Capital"": false,
                ""Cards"": [],
                ""Comments"": null,
                ""ConditionExpression"": ""[a9a7a6e3-ea53-41e9-8795-a7edb36ddb5d]=2"",
                ""Featured"": false,
                ""Instructions"": null,
                ""Mandatory"": false,
                ""PublicKey"": ""8364fecf-35e6-4a77-9049-2e5ed63f8f5d"",
                ""QuestionText"": ""If other specify"",
                ""QuestionType"": ""Text"",
                ""StataExportCaption"": null,
                ""ValidationExpression"": null,
                ""ValidationMessage"": null,
                ""Triggers"": null
              },
              {
                ""$type"": ""Main.Core.Entities.SubEntities.Question.AutoPropagateQuestion, Main.Core"",
                ""AddNumericAttr"": null,
                ""Children"": [],
                ""IntAttr"": 0,
                ""Triggers"": [
                  ""7995abad-41a3-45ac-bc33-40745674e998""
                ],
                ""AnswerOrder"": ""AsIs"",
                ""Capital"": false,
                ""Cards"": [],
                ""Comments"": null,
                ""ConditionExpression"": null,
                ""Featured"": false,
                ""Instructions"": null,
                ""Mandatory"": false,
                ""PublicKey"": ""9a717e52-cf9f-4964-bd7f-505e4a9e7e4e"",
                ""QuestionText"": ""How many rooms do you have?"",
                ""QuestionType"": ""AutoPropagate"",
                ""StataExportCaption"": ""dgdfg"",
                ""ValidationExpression"": null,
                ""ValidationMessage"": null
              }
            ],
            ""ConditionExpression"": """",
            ""Enabled"": false,
            ""Description"": """",
            ""Propagated"": ""None"",
            ""PublicKey"": ""2e070a63-1908-4f11-b816-4ea87c1ab35c"",
            ""Title"": ""HOUSE"",
            ""Triggers"": []
          },
          {
            ""$type"": ""Main.Core.Entities.SubEntities.Group, Main.Core"",
            ""Children"": [
              {
                ""$type"": ""Main.Core.Entities.SubEntities.Group, Main.Core"",
                ""Children"": [
                  {
                    ""$type"": ""Main.Core.Entities.SubEntities.Question.NumericQuestion, Main.Core"",
                    ""AddNumericAttr"": null,
                    ""Children"": [],
                    ""IntAttr"": 0,
                    ""AnswerOrder"": ""AsIs"",
                    ""Capital"": false,
                    ""Cards"": [],
                    ""Comments"": null,
                    ""ConditionExpression"": null,
                    ""Featured"": false,
                    ""Instructions"": null,
                    ""Mandatory"": false,
                    ""PublicKey"": ""b4817432-1e18-4437-bc4c-22a524fb1e71"",
                    ""QuestionText"": ""What is the area of room?"",
                    ""QuestionType"": ""Numeric"",
                    ""StataExportCaption"": null,
                    ""ValidationExpression"": null,
                    ""ValidationMessage"": null,
                    ""Triggers"": null
                  },
                  {
                    ""$type"": ""Main.Core.Entities.SubEntities.Question.SingleQuestion, Main.Core"",
                    ""AddSingleAttr"": null,
                    ""Children"": [
                      {
                        ""$type"": ""Main.Core.Entities.SubEntities.Answer, Main.Core"",
                        ""AnswerImage"": null,
                        ""AnswerText"": ""Living room"",
                        ""AnswerType"": ""Select"",
                        ""AnswerValue"": ""0"",
                        ""Children"": [],
                        ""Mandatory"": false,
                        ""NameCollection"": null,
                        ""PublicKey"": ""c826dad9-d76a-49d6-a40a-590fc0265b74""
                      },
                      {
                        ""$type"": ""Main.Core.Entities.SubEntities.Answer, Main.Core"",
                        ""AnswerImage"": null,
                        ""AnswerText"": ""Kitchen"",
                        ""AnswerType"": ""Select"",
                        ""AnswerValue"": ""1"",
                        ""Children"": [],
                        ""Mandatory"": false,
                        ""NameCollection"": null,
                        ""PublicKey"": ""737d0ec8-850b-4138-b239-8f5a1d0da874""
                      },
                      {
                        ""$type"": ""Main.Core.Entities.SubEntities.Answer, Main.Core"",
                        ""AnswerImage"": null,
                        ""AnswerText"": ""Bedroom"",
                        ""AnswerType"": ""Select"",
                        ""AnswerValue"": ""2"",
                        ""Children"": [],
                        ""Mandatory"": false,
                        ""NameCollection"": null,
                        ""PublicKey"": ""6f4f93b2-0e16-4a2f-8606-68465c383d0f""
                      },
                      {
                        ""$type"": ""Main.Core.Entities.SubEntities.Answer, Main.Core"",
                        ""AnswerImage"": null,
                        ""AnswerText"": ""Cabinet"",
                        ""AnswerType"": ""Select"",
                        ""AnswerValue"": ""3"",
                        ""Children"": [],
                        ""Mandatory"": false,
                        ""NameCollection"": null,
                        ""PublicKey"": ""9df57979-6578-4883-846e-6675333b2f9e""
                      }
                    ],
                    ""AnswerOrder"": ""AsIs"",
                    ""Capital"": true,
                    ""Cards"": [],
                    ""Comments"": null,
                    ""ConditionExpression"": null,
                    ""Featured"": false,
                    ""Instructions"": null,
                    ""Mandatory"": false,
                    ""PublicKey"": ""a61498b9-d0f2-4b11-a372-db6ed7ef6098"",
                    ""QuestionText"": ""Room type"",
                    ""QuestionType"": ""SingleOption"",
                    ""StataExportCaption"": ""room_type"",
                    ""ValidationExpression"": null,
                    ""ValidationMessage"": null,
                    ""Triggers"": null
                  }
                ],
                ""ConditionExpression"": """",
                ""Enabled"": false,
                ""Description"": """",
                ""Propagated"": ""AutoPropagated"",
                ""PublicKey"": ""7995abad-41a3-45ac-bc33-40745674e998"",
                ""Title"": ""Description of rooms"",
                ""Triggers"": []
              }
            ],
            ""ConditionExpression"": """",
            ""Enabled"": false,
            ""Description"": """",
            ""Propagated"": ""None"",
            ""PublicKey"": ""0ac2b813-e392-4462-a036-497fc3975a48"",
            ""Title"": ""HOUSE DETAILS"",
            ""Triggers"": []
          }
        ],
        ""CloseDate"": null,
        ""ConditionExpression"": """",
        ""CreationDate"": ""2012-04-24T11:50:14.1750000"",
        ""LastEntryDate"": ""2012-04-24T11:50:14.1750000"",
        ""OpenDate"": null,
        ""PublicKey"": ""2213d3cb-bf96-4c5f-813d-438759066c55"",
        ""Title"": ""2012 Research department general survey"",
        ""Description"": null,
        ""Triggers"": []
      }
    }
  }
}";

		}
		#endregion

		public static StoredEvent GetCreateTemplateEvent()
		{
			var serializedEventText = GetSerializedEvent();

			return JsonConvert.DeserializeObject<StoredEvent>(serializedEventText,
				new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects });
		}
	}
}