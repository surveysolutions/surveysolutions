// -----------------------------------------------------------------------
// <copyright file="QuestionnaireScreenViewFactory.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Complete;
using Main.Core.View;
using Newtonsoft.Json;

namespace AndroidApp.ViewModel.QuestionnaireDetails
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class QuestionnaireScreenViewFactory : IViewFactory<QuestionnaireScreenInput, QuestionnaireScreenViewModel>
    {
        private static CompleteQuestionnaireDocument root;
        static QuestionnaireScreenViewFactory()
        {
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };
            //var data = Encoding.Default.GetString("");
            var s =
                @"{" +
  @"      ""Children"": [" +
  @"        {" +
  @"          ""$type"": ""Main.Core.Entities.SubEntities.Complete.CompleteGroup, Main.Core""," +
  @"          ""Children"": [" +
  @"            {" +
  @"              ""$type"": ""Main.Core.Entities.SubEntities.Complete.Question.SingleCompleteQuestion, Main.Core""," +
  @"              ""AddSingleAttr"": null," +
  @"              ""AnswerDate"": ""2012-09-06T19:05:28.0640000""," +
  @"              ""Answers"": [" +
  @"                {" +
  @"                  ""$type"": ""Main.Core.Entities.SubEntities.Complete.CompleteAnswer, Main.Core""," +
  @"                  ""AnswerImage"": """"," +
  @"                  ""AnswerText"": ""Yes""," +
  @"                  ""AnswerType"": ""Select""," +
  @"                  ""AnswerValue"": ""1""," +
  @"                  ""Children"": []," +
  @"                  ""Mandatory"": false," +
  @"                  ""NameCollection"": null," +
  @"                  ""PropogationPublicKey"": null," +
  @"                  ""PublicKey"": ""13f206a2-b762-4877-9210-00984883e6c8""," +
  @"                  ""Selected"": false" +
  @"                }," +
  @"                {" +
  @"                  ""$type"": ""Main.Core.Entities.SubEntities.Complete.CompleteAnswer, Main.Core""," +
  @"                  ""AnswerImage"": """"," +
  @"                  ""AnswerText"": ""No""," +
  @"                  ""AnswerType"": ""Select""," +
  @"                  ""AnswerValue"": ""0""," +
  @"                  ""Children"": []," +
  @"                  ""Mandatory"": false," +
  @"                  ""NameCollection"": null," +
  @"                  ""PropogationPublicKey"": null," +
  @"                  ""PublicKey"": ""65e574e2-72b0-4bcf-8f33-6bdd7f8ee916""," +
  @"                  ""Selected"": false" +
  @"                }" +
  @"              ]," +
  @"              ""AnswerOrder"": ""AsIs""," +
  @"              ""Capital"": false," +
  @"              ""Cards"": []," +
  @"              ""Children"": null," +
  @"              ""Comments"": null," +
  @"              ""ConditionExpression"": null," +
  @"              ""Enabled"": true," +
  @"              ""Featured"": false," +
  @"              ""Instructions"": null," +
  @"              ""Mandatory"": false," +
  @"              ""PropagationPublicKey"": null," +
  @"              ""PublicKey"": ""77362eae-c572-4680-bf7c-773246199e3c""," +
  @"              ""QuestionText"": ""Do you khow this man? <img src=\""../../Content/img/bill.jpg\""/>""," +
  @"              ""QuestionType"": ""SingleOption""," +
  @"              ""StataExportCaption"": null," +
  @"              ""Valid"": true," +
  @"              ""ValidationExpression"": null," +
  @"              ""ValidationMessage"": null," +
  @"              ""Triggers"": []" +
  @"            }," +
   @"        {  ""$type"": ""Main.Core.Entities.SubEntities.Complete.CompleteGroup, Main.Core""," +
 @"          ""Children"": [" +
  @"{" +
  @"              ""$type"": ""Main.Core.Entities.SubEntities.Complete.Question.SingleCompleteQuestion, Main.Core""," +
  @"              ""AddSingleAttr"": null," +
  @"              ""AnswerDate"": ""2012-09-06T19:05:28.0640000""," +
  @"              ""Answers"": [" +
  @"                {" +
  @"                  ""$type"": ""Main.Core.Entities.SubEntities.Complete.CompleteAnswer, Main.Core""," +
  @"                  ""AnswerImage"": null," +
  @"                  ""AnswerText"": ""12-18""," +
  @"                  ""AnswerType"": ""Select""," +
  @"                  ""AnswerValue"": null," +
  @"                  ""Children"": []," +
  @"                  ""Mandatory"": false," +
  @"                  ""NameCollection"": null," +
  @"                  ""PropogationPublicKey"": null," +
  @"                  ""PublicKey"": ""c1cd8569-eef5-43f3-a7bb-4d8fdf171f03""," +
  @"                  ""Selected"": false" +
  @"                }," +
  @"                {" +
  @"                  ""$type"": ""Main.Core.Entities.SubEntities.Complete.CompleteAnswer, Main.Core""," +
  @"                  ""AnswerImage"": null," +
  @"                  ""AnswerText"": ""19-23""," +
  @"                  ""AnswerType"": ""Select""," +
  @"                  ""AnswerValue"": null," +
  @"                  ""Children"": []," +
  @"                  ""Mandatory"": false," +
  @"                  ""NameCollection"": null," +
  @"                  ""PropogationPublicKey"": null," +
  @"                  ""PublicKey"": ""8e8f4b72-ed75-4f70-b1f5-7892d3094a3a""," +
  @"                  ""Selected"": false" +
  @"                }," +
  @"                {" +
  @"                  ""$type"": ""Main.Core.Entities.SubEntities.Complete.CompleteAnswer, Main.Core""," +
  @"                  ""AnswerImage"": null," +
  @"                  ""AnswerText"": ""24-35""," +
  @"                  ""AnswerType"": ""Select""," +
  @"                  ""AnswerValue"": null," +
  @"                  ""Children"": []," +
  @"                  ""Mandatory"": false," +
  @"                  ""NameCollection"": null," +
  @"                  ""PropogationPublicKey"": null," +
  @"                  ""PublicKey"": ""94f915c8-096b-4b25-8a15-36d8a235c982""," +
  @"                  ""Selected"": false" +
  @"                }," +
  @"                {" +
  @"                  ""$type"": ""Main.Core.Entities.SubEntities.Complete.CompleteAnswer, Main.Core""," +
  @"                  ""AnswerImage"": null," +
  @"                  ""AnswerText"": ""36-...""," +
  @"                  ""AnswerType"": ""Select""," +
  @"                  ""AnswerValue"": null," +
  @"                  ""Children"": []," +
  @"                  ""Mandatory"": false," +
  @"                  ""NameCollection"": null," +
  @"                  ""PropogationPublicKey"": null," +
  @"                  ""PublicKey"": ""ace01169-eea0-4cd4-9ced-eabdf5acc0f4""," +
  @"                  ""Selected"": false" +
  @"                }" +
  @"              ]," +
  @"              ""AnswerOrder"": ""AsIs""," +
  @"              ""Capital"": false," +
  @"              ""Cards"": []," +
  @"              ""Children"": null," +
  @"              ""Comments"": null," +
  @"              ""ConditionExpression"": null," +
  @"              ""Enabled"": true," +
  @"              ""Featured"": true," +
  @"              ""Instructions"": null," +
  @"              ""Mandatory"": false," +
  @"              ""PropagationPublicKey"": null," +
  @"              ""PublicKey"": ""011e4778-4f21-4f60-b4b3-353fb977ec27""," +
  @"              ""QuestionText"": ""Respondent's age:""," +
  @"              ""QuestionType"": ""SingleOption""," +
  @"              ""StataExportCaption"": null," +
  @"              ""Valid"": true," +
  @"              ""ValidationExpression"": null," +
  @"              ""ValidationMessage"": null," +
  @"              ""Triggers"": []" +
  @"            }," +
  @"            {" +
  @"              ""$type"": ""Main.Core.Entities.SubEntities.Complete.Question.SingleCompleteQuestion, Main.Core""," +
  @"              ""AddSingleAttr"": null," +
  @"              ""AnswerDate"": ""2012-09-06T19:05:28.0640000""," +
  @"              ""Answers"": [" +
  @"                {" +
  @"                  ""$type"": ""Main.Core.Entities.SubEntities.Complete.CompleteAnswer, Main.Core""," +
  @"                  ""AnswerImage"": null," +
  @"                  ""AnswerText"": ""University degree""," +
  @"                  ""AnswerType"": ""Select""," +
  @"                  ""AnswerValue"": null," +
  @"                  ""Children"": []," +
  @"                  ""Mandatory"": false," +
  @"                  ""NameCollection"": null," +
  @"                  ""PropogationPublicKey"": null," +
  @"                  ""PublicKey"": ""4c65ce9f-4a86-4070-be05-531e2cad8ac3""," +
  @"                  ""Selected"": false" +
  @"                }," +
  @"                {" +
  @"                  ""$type"": ""Main.Core.Entities.SubEntities.Complete.CompleteAnswer, Main.Core""," +
  @"                  ""AnswerImage"": null," +
  @"                  ""AnswerText"": ""Primary""," +
  @"                  ""AnswerType"": ""Select""," +
  @"                  ""AnswerValue"": null," +
  @"                  ""Children"": []," +
  @"                  ""Mandatory"": false," +
  @"                  ""NameCollection"": null," +
  @"                  ""PropogationPublicKey"": null," +
  @"                  ""PublicKey"": ""cad0ecfd-b5cd-43d4-a0bb-d59be07a0e78""," +
  @"                  ""Selected"": false" +
  @"                }," +
  @"                {" +
  @"                  ""$type"": ""Main.Core.Entities.SubEntities.Complete.CompleteAnswer, Main.Core""," +
  @"                  ""AnswerImage"": null," +
  @"                  ""AnswerText"": ""Illiterate""," +
  @"                  ""AnswerType"": ""Select""," +
  @"                  ""AnswerValue"": null," +
  @"                  ""Children"": []," +
  @"                  ""Mandatory"": false," +
  @"                  ""NameCollection"": null," +
  @"                  ""PropogationPublicKey"": null," +
  @"                  ""PublicKey"": ""53a8511b-37ba-40f6-86f3-ec6dc8efea44""," +
  @"                  ""Selected"": false" +
  @"                }," +
  @"                {" +
  @"                  ""$type"": ""Main.Core.Entities.SubEntities.Complete.CompleteAnswer, Main.Core""," +
  @"                  ""AnswerImage"": null," +
  @"                  ""AnswerText"": ""Secondary""," +
  @"                  ""AnswerType"": ""Select""," +
  @"                  ""AnswerValue"": null," +
  @"                  ""Children"": []," +
  @"                  ""Mandatory"": false," +
  @"                  ""NameCollection"": null," +
  @"                  ""PropogationPublicKey"": null," +
  @"                  ""PublicKey"": ""beb7d33a-1ffa-4f04-b3eb-0e0ee623e710""," +
  @"                  ""Selected"": false" +
  @"                }," +
  @"                {" +
  @"                  ""$type"": ""Main.Core.Entities.SubEntities.Complete.CompleteAnswer, Main.Core""," +
  @"                  ""AnswerImage"": null," +
  @"                  ""AnswerText"": ""High school diploma""," +
  @"                  ""AnswerType"": ""Select""," +
  @"                  ""AnswerValue"": null," +
  @"                  ""Children"": []," +
  @"                  ""Mandatory"": false," +
  @"                  ""NameCollection"": null," +
  @"                  ""PropogationPublicKey"": null," +
  @"                  ""PublicKey"": ""ab1a577c-1c37-4a61-b76a-b3c528ff7e61""," +
  @"                  ""Selected"": false" +
  @"                }," +
  @"                {" +
  @"                  ""$type"": ""Main.Core.Entities.SubEntities.Complete.CompleteAnswer, Main.Core""," +
  @"                  ""AnswerImage"": null," +
  @"                  ""AnswerText"": ""Ph.D.""," +
  @"                  ""AnswerType"": ""Select""," +
  @"                  ""AnswerValue"": null," +
  @"                  ""Children"": []," +
  @"                  ""Mandatory"": false," +
  @"                  ""NameCollection"": null," +
  @"                  ""PropogationPublicKey"": null," +
  @"                  ""PublicKey"": ""344d6d33-9f5e-415d-b973-d0f2fe226c65""," +
  @"                  ""Selected"": false" +
  @"                }," +
  @"                {" +
  @"                  ""$type"": ""Main.Core.Entities.SubEntities.Complete.CompleteAnswer, Main.Core""," +
  @"                  ""AnswerImage"": null," +
  @"                  ""AnswerText"": ""Masters""," +
  @"                  ""AnswerType"": ""Select""," +
  @"                  ""AnswerValue"": null," +
  @"                  ""Children"": []," +
  @"                  ""Mandatory"": false," +
  @"                  ""NameCollection"": null," +
  @"                  ""PropogationPublicKey"": null," +
  @"                  ""PublicKey"": ""905699cc-d66e-44c5-ab31-137174ec3791""," +
  @"                  ""Selected"": false" +
  @"                }" +
  @"              ]," +
  @"              ""AnswerOrder"": ""Random""," +
  @"              ""Capital"": false," +
  @"              ""Cards"": []," +
  @"              ""Children"": null," +
  @"              ""Comments"": null," +
  @"              ""ConditionExpression"": null," +
  @"              ""Enabled"": true," +
  @"              ""Featured"": true," +
  @"              ""Instructions"": null," +
  @"              ""Mandatory"": false," +
  @"              ""PropagationPublicKey"": null," +
  @"              ""PublicKey"": ""750e0019-047a-4fbd-a69d-8821ee6ba12b""," +
  @"              ""QuestionText"": ""Respondent's highest level of education:""," +
  @"              ""QuestionType"": ""SingleOption""," +
  @"              ""StataExportCaption"": null," +
  @"              ""Valid"": true," +
  @"              ""ValidationExpression"": null," +
  @"              ""ValidationMessage"": null," +
  @"              ""Triggers"": []" +
  @"            }," +
  @"            {" +
  @"              ""$type"": ""Main.Core.Entities.SubEntities.Complete.Question.TextCompleteQuestion, Main.Core""," +
  @"              ""AddTextAttr"": null," +
  @"              ""Answer"": null," +
  @"              ""AnswerDate"": ""2012-09-06T19:05:28.0640000""," +
  @"              ""Answers"": []," +
  @"              ""AnswerOrder"": ""AsIs""," +
  @"              ""Capital"": false," +
  @"              ""Cards"": []," +
  @"              ""Children"": null," +
  @"              ""Comments"": null," +
  @"              ""ConditionExpression"": null," +
  @"              ""Enabled"": true," +
  @"              ""Featured"": true," +
  @"              ""Instructions"": null," +
  @"              ""Mandatory"": false," +
  @"              ""PropagationPublicKey"": null," +
  @"              ""PublicKey"": ""3d9f69ac-1670-4ace-9715-30dcea62fd2e""," +
  @"              ""QuestionText"": ""Respondent's name of school""," +
  @"              ""QuestionType"": ""Text""," +
  @"              ""StataExportCaption"": null," +
  @"              ""Valid"": true," +
  @"              ""ValidationExpression"": null," +
  @"              ""ValidationMessage"": null," +
  @"              ""Triggers"": []" +
  @"            }" +
  
  
  @"          ]," +
  @"          ""ConditionExpression"": """"," +
  @"          ""Enabled"": true," +
  @"          ""Description"": null," +
  @"          ""Propagated"": ""None""," +
  @"          ""PropagationPublicKey"": null," +
  @"          ""PublicKey"": ""93f98cd4-11ef-44c4-b5bd-5c5a739c691c""," +
  @"          ""Title"": ""SubGroup""," +
  @"          ""Triggers"": []" +
  @"        }"+
  @"          ]," +
  @"          ""ConditionExpression"": """"," +
  @"          ""Enabled"": true," +
  @"          ""Description"": null," +
  @"          ""Propagated"": ""None""," +
  @"          ""PropagationPublicKey"": null," +
  @"          ""PublicKey"": ""53279073-5b77-4ab8-954c-6f949430cc80""," +
  @"          ""Title"": ""GENERAL""," +
  @"          ""Triggers"": []" +
  @"        }," +
  @"        {" +
  @"          ""$type"": ""Main.Core.Entities.SubEntities.Complete.CompleteGroup, Main.Core""," +
  @"          ""Children"": [" +
  @"            {" +
  @"              ""$type"": ""Main.Core.Entities.SubEntities.Complete.Question.SingleCompleteQuestion, Main.Core""," +
  @"              ""AddSingleAttr"": null," +
  @"              ""AnswerDate"": ""2012-09-06T19:05:28.0640000""," +
  @"              ""Answers"": [" +
  @"                {" +
  @"                  ""$type"": ""Main.Core.Entities.SubEntities.Complete.CompleteAnswer, Main.Core""," +
  @"                  ""AnswerImage"": """"," +
  @"                  ""AnswerText"": ""Yes""," +
  @"                  ""AnswerType"": ""Select""," +
  @"                  ""AnswerValue"": ""1""," +
  @"                  ""Children"": []," +
  @"                  ""Mandatory"": false," +
  @"                  ""NameCollection"": null," +
  @"                  ""PropogationPublicKey"": null," +
  @"                  ""PublicKey"": ""7ca88193-aec2-4315-8647-9db365e122d9""," +
  @"                  ""Selected"": false" +
  @"                }," +
  @"                {" +
  @"                  ""$type"": ""Main.Core.Entities.SubEntities.Complete.CompleteAnswer, Main.Core""," +
  @"                  ""AnswerImage"": """"," +
  @"                  ""AnswerText"": ""No""," +
  @"                  ""AnswerType"": ""Select""," +
  @"                  ""AnswerValue"": ""0""," +
  @"                  ""Children"": []," +
  @"                  ""Mandatory"": false," +
  @"                  ""NameCollection"": null," +
  @"                  ""PropogationPublicKey"": null," +
  @"                  ""PublicKey"": ""47853bdf-420e-476c-adcb-094f8849f001""," +
  @"                  ""Selected"": false" +
  @"                }" +
  @"              ]," +
  @"              ""AnswerOrder"": ""AsIs""," +
  @"              ""Capital"": false," +
  @"              ""Cards"": []," +
  @"              ""Children"": null," +
  @"              ""Comments"": null," +
  @"              ""ConditionExpression"": null," +
  @"              ""Enabled"": true," +
  @"              ""Featured"": false," +
  @"              ""Instructions"": null," +
  @"              ""Mandatory"": false," +
  @"              ""PropagationPublicKey"": null," +
  @"              ""PublicKey"": ""4d1405ad-352a-4116-a26a-47e824fbb4cf""," +
  @"              ""QuestionText"": ""Do you have a pet?""," +
  @"              ""QuestionType"": ""YesNo""," +
  @"              ""StataExportCaption"": null," +
  @"              ""Valid"": true," +
  @"              ""ValidationExpression"": null," +
  @"              ""ValidationMessage"": null," +
  @"              ""Triggers"": []" +
  @"            }," +
  @"            {" +
  @"              ""$type"": ""Main.Core.Entities.SubEntities.Complete.Question.DateTimeCompleteQuestion, Main.Core""," +
  @"              ""AddDateTimeAttr"": null," +
  @"              ""DateTimeAttr"": ""0001-01-01T00:00:00.0000000""," +
  @"              ""Answer"": null," +
  @"              ""AnswerDate"": ""2012-09-06T19:05:28.0640000""," +
  @"              ""Answers"": []," +
  @"              ""AnswerOrder"": ""AsIs""," +
  @"              ""Capital"": false," +
  @"              ""Cards"": []," +
  @"              ""Children"": null," +
  @"              ""Comments"": null," +
  @"              ""ConditionExpression"": ""[4d1405ad-352a-4116-a26a-47e824fbb4cf]=1""," +
  @"              ""Enabled"": true," +
  @"              ""Featured"": false," +
  @"              ""Instructions"": null," +
  @"              ""Mandatory"": false," +
  @"              ""PropagationPublicKey"": null," +
  @"              ""PublicKey"": ""18f3b435-60b5-4a69-bbf5-d915809efc2b""," +
  @"              ""QuestionText"": ""What is it date of birth""," +
  @"              ""QuestionType"": ""DateTime""," +
  @"              ""StataExportCaption"": null," +
  @"              ""Valid"": true," +
  @"              ""ValidationExpression"": null," +
  @"              ""ValidationMessage"": null," +
  @"              ""Triggers"": [" +
  @"                ""4d1405ad-352a-4116-a26a-47e824fbb4cf""" +
  @"              ]" +
  @"            }," +
  @"            {" +
  @"              ""$type"": ""Main.Core.Entities.SubEntities.Complete.Question.SingleCompleteQuestion, Main.Core""," +
  @"              ""AddSingleAttr"": null," +
  @"              ""AnswerDate"": ""2012-09-06T19:05:28.0640000""," +
  @"              ""Answers"": [" +
  @"                {" +
  @"                  ""$type"": ""Main.Core.Entities.SubEntities.Complete.CompleteAnswer, Main.Core""," +
  @"                  ""AnswerImage"": """"," +
  @"                  ""AnswerText"": ""Yes""," +
  @"                  ""AnswerType"": ""Select""," +
  @"                  ""AnswerValue"": ""1""," +
  @"                  ""Children"": []," +
  @"                  ""Mandatory"": false," +
  @"                  ""NameCollection"": null," +
  @"                  ""PropogationPublicKey"": null," +
  @"                  ""PublicKey"": ""89c68eb6-c3f5-49d9-94f9-53b7fff6f9cc""," +
  @"                  ""Selected"": false" +
  @"                }," +
  @"                {" +
  @"                  ""$type"": ""Main.Core.Entities.SubEntities.Complete.CompleteAnswer, Main.Core""," +
  @"                  ""AnswerImage"": """"," +
  @"                  ""AnswerText"": ""No""," +
  @"                  ""AnswerType"": ""Select""," +
  @"                  ""AnswerValue"": ""0""," +
  @"                  ""Children"": []," +
  @"                  ""Mandatory"": false," +
  @"                  ""NameCollection"": null," +
  @"                  ""PropogationPublicKey"": null," +
  @"                  ""PublicKey"": ""a9c00784-f6b3-4faf-ab32-468c4f22ac74""," +
  @"                  ""Selected"": false" +
  @"                }" +
  @"              ]," +
  @"              ""AnswerOrder"": ""AsIs""," +
  @"              ""Capital"": false," +
  @"              ""Cards"": []," +
  @"              ""Children"": null," +
  @"              ""Comments"": null," +
  @"              ""ConditionExpression"": ""[4d1405ad-352a-4116-a26a-47e824fbb4cf]=1""," +
  @"              ""Enabled"": true," +
  @"              ""Featured"": false," +
  @"              ""Instructions"": null," +
  @"              ""Mandatory"": false," +
  @"              ""PropagationPublicKey"": null," +
  @"              ""PublicKey"": ""90f3cf5a-0fd1-4e33-8a2c-36365e905752""," +
  @"              ""QuestionText"": ""Does it live with you?""," +
  @"              ""QuestionType"": ""YesNo""," +
  @"              ""StataExportCaption"": null," +
  @"              ""Valid"": true," +
  @"              ""ValidationExpression"": null," +
  @"              ""ValidationMessage"": null," +
  @"              ""Triggers"": [" +
  @"                ""4d1405ad-352a-4116-a26a-47e824fbb4cf""," +
  @"                ""4d1405ad-352a-4116-a26a-47e824fbb4cf""," +
  @"                ""4d1405ad-352a-4116-a26a-47e824fbb4cf""," +
  @"                ""4d1405ad-352a-4116-a26a-47e824fbb4cf""," +
  @"                ""4d1405ad-352a-4116-a26a-47e824fbb4cf""," +
  @"                ""4d1405ad-352a-4116-a26a-47e824fbb4cf""," +
  @"                ""4d1405ad-352a-4116-a26a-47e824fbb4cf""" +
  @"              ]" +
  @"            }," +
  @"            {" +
  @"              ""$type"": ""Main.Core.Entities.SubEntities.Complete.Question.NumericCompleteQuestion, Main.Core""," +
  @"              ""AddNumericAttr"": null," +
  @"              ""IntAttr"": 0," +
  @"              ""Answer"": null," +
  @"              ""AnswerDate"": ""2012-09-06T19:05:28.0640000""," +
  @"              ""Answers"": []," +
  @"              ""AnswerOrder"": ""AsIs""," +
  @"              ""Capital"": false," +
  @"              ""Cards"": []," +
  @"              ""Children"": null," +
  @"              ""Comments"": null," +
  @"              ""ConditionExpression"": ""[4d1405ad-352a-4116-a26a-47e824fbb4cf]=1&&[90f3cf5a-0fd1-4e33-8a2c-36365e905752]=1""," +
  @"              ""Enabled"": true," +
  @"              ""Featured"": false," +
  @"              ""Instructions"": null," +
  @"              ""Mandatory"": false," +
  @"              ""PropagationPublicKey"": null," +
  @"              ""PublicKey"": ""3482cee6-548d-41c3-a024-db94fedc5873""," +
  @"              ""QuestionText"": ""How many times per day do you walk your pet?""," +
  @"              ""QuestionType"": ""Numeric""," +
  @"              ""StataExportCaption"": null," +
  @"              ""Valid"": true," +
  @"              ""ValidationExpression"": null," +
  @"              ""ValidationMessage"": null," +
  @"              ""Triggers"": [" +
  @"                ""4d1405ad-352a-4116-a26a-47e824fbb4cf""," +
  @"                ""90f3cf5a-0fd1-4e33-8a2c-36365e905752""," +
  @"                ""4d1405ad-352a-4116-a26a-47e824fbb4cf""," +
  @"                ""90f3cf5a-0fd1-4e33-8a2c-36365e905752""," +
  @"                ""4d1405ad-352a-4116-a26a-47e824fbb4cf""," +
  @"                ""90f3cf5a-0fd1-4e33-8a2c-36365e905752""," +
  @"                ""4d1405ad-352a-4116-a26a-47e824fbb4cf""," +
  @"                ""90f3cf5a-0fd1-4e33-8a2c-36365e905752""," +
  @"                ""4d1405ad-352a-4116-a26a-47e824fbb4cf""," +
  @"                ""90f3cf5a-0fd1-4e33-8a2c-36365e905752""," +
  @"                ""4d1405ad-352a-4116-a26a-47e824fbb4cf""," +
  @"                ""90f3cf5a-0fd1-4e33-8a2c-36365e905752""," +
  @"                ""4d1405ad-352a-4116-a26a-47e824fbb4cf""," +
  @"                ""90f3cf5a-0fd1-4e33-8a2c-36365e905752""" +
  @"              ]" +
  @"            }," +
  @"            {" +
  @"              ""$type"": ""Main.Core.Entities.SubEntities.Complete.Question.TextCompleteQuestion, Main.Core""," +
  @"              ""AddTextAttr"": null," +
  @"              ""Answer"": null," +
  @"              ""AnswerDate"": ""2012-09-06T19:05:28.0640000""," +
  @"              ""Answers"": []," +
  @"              ""AnswerOrder"": ""AsIs""," +
  @"              ""Capital"": false," +
  @"              ""Cards"": []," +
  @"              ""Children"": null," +
  @"              ""Comments"": null," +
  @"              ""ConditionExpression"": ""[4d1405ad-352a-4116-a26a-47e824fbb4cf]=1""," +
  @"              ""Enabled"": true," +
  @"              ""Featured"": false," +
  @"              ""Instructions"": null," +
  @"              ""Mandatory"": false," +
  @"              ""PropagationPublicKey"": null," +
  @"              ""PublicKey"": ""20820feb-ccd4-4049-8bc6-2018e222b4f1""," +
  @"              ""QuestionText"": ""What is your pet name?""," +
  @"              ""QuestionType"": ""Text""," +
  @"              ""StataExportCaption"": null," +
  @"              ""Valid"": true," +
  @"              ""ValidationExpression"": null," +
  @"              ""ValidationMessage"": null," +
  @"              ""Triggers"": [" +
  @"                ""4d1405ad-352a-4116-a26a-47e824fbb4cf""," +
  @"                ""4d1405ad-352a-4116-a26a-47e824fbb4cf""," +
  @"                ""4d1405ad-352a-4116-a26a-47e824fbb4cf""," +
  @"                ""4d1405ad-352a-4116-a26a-47e824fbb4cf""," +
  @"                ""4d1405ad-352a-4116-a26a-47e824fbb4cf""," +
  @"                ""4d1405ad-352a-4116-a26a-47e824fbb4cf""," +
  @"                ""4d1405ad-352a-4116-a26a-47e824fbb4cf""" +
  @"              ]" +
  @"            }" +
  @"          ]," +
  @"          ""ConditionExpression"": """"," +
  @"          ""Enabled"": true," +
  @"          ""Description"": null," +
  @"          ""Propagated"": ""None""," +
  @"          ""PropagationPublicKey"": null," +
  @"          ""PublicKey"": ""1068cac2-8fd2-43bb-bc57-04607364152e""," +
  @"          ""Title"": ""PETS""," +
  @"          ""Triggers"": []" +
  @"        }," +
  @"        {" +
  @"          ""$type"": ""Main.Core.Entities.SubEntities.Complete.CompleteGroup, Main.Core""," +
  @"          ""Children"": [" +
  @"            {" +
  @"              ""$type"": ""Main.Core.Entities.SubEntities.Complete.Question.SingleCompleteQuestion, Main.Core""," +
  @"              ""AddSingleAttr"": null," +
  @"              ""AnswerDate"": ""2012-09-06T19:05:28.0640000""," +
  @"              ""Answers"": [" +
  @"                {" +
  @"                  ""$type"": ""Main.Core.Entities.SubEntities.Complete.CompleteAnswer, Main.Core""," +
  @"                  ""AnswerImage"": """"," +
  @"                  ""AnswerText"": ""Yes""," +
  @"                  ""AnswerType"": ""Select""," +
  @"                  ""AnswerValue"": ""1""," +
  @"                  ""Children"": []," +
  @"                  ""Mandatory"": false," +
  @"                  ""NameCollection"": null," +
  @"                  ""PropogationPublicKey"": null," +
  @"                  ""PublicKey"": ""af008918-3308-4730-b6b7-13bf8a94e571""," +
  @"                  ""Selected"": false" +
  @"                }," +
  @"                {" +
  @"                  ""$type"": ""Main.Core.Entities.SubEntities.Complete.CompleteAnswer, Main.Core""," +
  @"                  ""AnswerImage"": """"," +
  @"                  ""AnswerText"": ""No""," +
  @"                  ""AnswerType"": ""Select""," +
  @"                  ""AnswerValue"": ""0""," +
  @"                  ""Children"": []," +
  @"                  ""Mandatory"": false," +
  @"                  ""NameCollection"": null," +
  @"                  ""PropogationPublicKey"": null," +
  @"                  ""PublicKey"": ""bd6b2ebd-d485-4f95-a287-d79a66aa2e5f""," +
  @"                  ""Selected"": false" +
  @"                }" +
  @"              ]," +
  @"              ""AnswerOrder"": ""AsIs""," +
  @"              ""Capital"": false," +
  @"              ""Cards"": []," +
  @"              ""Children"": null," +
  @"              ""Comments"": null," +
  @"              ""ConditionExpression"": null," +
  @"              ""Enabled"": true," +
  @"              ""Featured"": false," +
  @"              ""Instructions"": null," +
  @"              ""Mandatory"": false," +
  @"              ""PropagationPublicKey"": null," +
  @"              ""PublicKey"": ""879df044-c8ca-442f-a0d8-aa967c53281e""," +
  @"              ""QuestionText"": ""Do you like the World Bank cafeteria?""," +
  @"              ""QuestionType"": ""YesNo""," +
  @"              ""StataExportCaption"": null," +
  @"              ""Valid"": true," +
  @"              ""ValidationExpression"": null," +
  @"              ""ValidationMessage"": null," +
  @"              ""Triggers"": []" +
  @"            }," +
  @"            {" +
  @"              ""$type"": ""Main.Core.Entities.SubEntities.Complete.Question.SingleCompleteQuestion, Main.Core""," +
  @"              ""AddSingleAttr"": null," +
  @"              ""AnswerDate"": ""2012-09-06T19:05:28.0640000""," +
  @"              ""Answers"": [" +
  @"                {" +
  @"                  ""$type"": ""Main.Core.Entities.SubEntities.Complete.CompleteAnswer, Main.Core""," +
  @"                  ""AnswerImage"": """"," +
  @"                  ""AnswerText"": ""a. Service""," +
  @"                  ""AnswerType"": ""Select""," +
  @"                  ""AnswerValue"": null," +
  @"                  ""Children"": []," +
  @"                  ""Mandatory"": false," +
  @"                  ""NameCollection"": null," +
  @"                  ""PropogationPublicKey"": null," +
  @"                  ""PublicKey"": ""5f6664e7-5797-4bae-a3ed-0b8f6ed7fb52""," +
  @"                  ""Selected"": false" +
  @"                }," +
  @"                {" +
  @"                  ""$type"": ""Main.Core.Entities.SubEntities.Complete.CompleteAnswer, Main.Core""," +
  @"                  ""AnswerImage"": """"," +
  @"                  ""AnswerText"": ""b. Menu""," +
  @"                  ""AnswerType"": ""Select""," +
  @"                  ""AnswerValue"": null," +
  @"                  ""Children"": []," +
  @"                  ""Mandatory"": false," +
  @"                  ""NameCollection"": null," +
  @"                  ""PropogationPublicKey"": null," +
  @"                  ""PublicKey"": ""ed965d26-5ea2-479d-8ebe-128574f275f6""," +
  @"                  ""Selected"": false" +
  @"                }," +
  @"                {" +
  @"                  ""$type"": ""Main.Core.Entities.SubEntities.Complete.CompleteAnswer, Main.Core""," +
  @"                  ""AnswerImage"": """"," +
  @"                  ""AnswerText"": ""c. Prices""," +
  @"                  ""AnswerType"": ""Select""," +
  @"                  ""AnswerValue"": null," +
  @"                  ""Children"": []," +
  @"                  ""Mandatory"": false," +
  @"                  ""NameCollection"": null," +
  @"                  ""PropogationPublicKey"": null," +
  @"                  ""PublicKey"": ""02a7931d-887d-4c9a-b3a3-01f6e3b49bfe""," +
  @"                  ""Selected"": false" +
  @"                }," +
  @"                {" +
  @"                  ""$type"": ""Main.Core.Entities.SubEntities.Complete.CompleteAnswer, Main.Core""," +
  @"                  ""AnswerImage"": """"," +
  @"                  ""AnswerText"": ""d. Location""," +
  @"                  ""AnswerType"": ""Select""," +
  @"                  ""AnswerValue"": null," +
  @"                  ""Children"": []," +
  @"                  ""Mandatory"": false," +
  @"                  ""NameCollection"": null," +
  @"                  ""PropogationPublicKey"": null," +
  @"                  ""PublicKey"": ""b76dc690-86b7-4f9c-ac6a-e5e67c750e09""," +
  @"                  ""Selected"": false" +
  @"                }" +
  @"              ]," +
  @"              ""AnswerOrder"": ""AsIs""," +
  @"              ""Capital"": false," +
  @"              ""Cards"": []," +
  @"              ""Children"": null," +
  @"              ""Comments"": null," +
  @"              ""ConditionExpression"": null," +
  @"              ""Enabled"": true," +
  @"              ""Featured"": false," +
  @"              ""Instructions"": null," +
  @"              ""Mandatory"": false," +
  @"              ""PropagationPublicKey"": null," +
  @"              ""PublicKey"": ""a3ce15ab-0745-4ee6-bb19-7e91f691c75a""," +
  @"              ""QuestionText"": ""What is the best station in the cafeteria?""," +
  @"              ""QuestionType"": ""SingleOption""," +
  @"              ""StataExportCaption"": null," +
  @"              ""Valid"": true," +
  @"              ""ValidationExpression"": null," +
  @"              ""ValidationMessage"": null," +
  @"              ""Triggers"": []" +
  @"            }," +
  @"            {" +
  @"              ""$type"": ""Main.Core.Entities.SubEntities.Complete.Question.SingleCompleteQuestion, Main.Core""," +
  @"              ""AddSingleAttr"": null," +
  @"              ""AnswerDate"": ""2012-09-06T19:05:28.0640000""," +
  @"              ""Answers"": [" +
  @"                {" +
  @"                  ""$type"": ""Main.Core.Entities.SubEntities.Complete.CompleteAnswer, Main.Core""," +
  @"                  ""AnswerImage"": """"," +
  @"                  ""AnswerText"": ""a. Service""," +
  @"                  ""AnswerType"": ""Select""," +
  @"                  ""AnswerValue"": null," +
  @"                  ""Children"": []," +
  @"                  ""Mandatory"": false," +
  @"                  ""NameCollection"": null," +
  @"                  ""PropogationPublicKey"": null," +
  @"                  ""PublicKey"": ""8a7c32fd-fea7-4fd3-b956-2e8dea42fbc9""," +
  @"                  ""Selected"": false" +
  @"                }," +
  @"                {" +
  @"                  ""$type"": ""Main.Core.Entities.SubEntities.Complete.CompleteAnswer, Main.Core""," +
  @"                  ""AnswerImage"": """"," +
  @"                  ""AnswerText"": ""b. Menu""," +
  @"                  ""AnswerType"": ""Select""," +
  @"                  ""AnswerValue"": null," +
  @"                  ""Children"": []," +
  @"                  ""Mandatory"": false," +
  @"                  ""NameCollection"": null," +
  @"                  ""PropogationPublicKey"": null," +
  @"                  ""PublicKey"": ""bd6e4785-f5d8-4306-afb6-62e5ea72f316""," +
  @"                  ""Selected"": false" +
  @"                }," +
  @"                {" +
  @"                  ""$type"": ""Main.Core.Entities.SubEntities.Complete.CompleteAnswer, Main.Core""," +
  @"                  ""AnswerImage"": """"," +
  @"                  ""AnswerText"": ""c. Prices""," +
  @"                  ""AnswerType"": ""Select""," +
  @"                  ""AnswerValue"": null," +
  @"                  ""Children"": []," +
  @"                  ""Mandatory"": false," +
  @"                  ""NameCollection"": null," +
  @"                  ""PropogationPublicKey"": null," +
  @"                  ""PublicKey"": ""9675854e-ba8f-4e82-aa1a-86eaa72ce54d""," +
  @"                  ""Selected"": false" +
  @"                }," +
  @"                {" +
  @"                  ""$type"": ""Main.Core.Entities.SubEntities.Complete.CompleteAnswer, Main.Core""," +
  @"                  ""AnswerImage"": """"," +
  @"                  ""AnswerText"": ""d. Location""," +
  @"                  ""AnswerType"": ""Select""," +
  @"                  ""AnswerValue"": null," +
  @"                  ""Children"": []," +
  @"                  ""Mandatory"": false," +
  @"                  ""NameCollection"": null," +
  @"                  ""PropogationPublicKey"": null," +
  @"                  ""PublicKey"": ""3e6aab03-0c7d-4b43-baa8-c2a17bcd467a""," +
  @"                  ""Selected"": false" +
  @"                }" +
  @"              ]," +
  @"              ""AnswerOrder"": ""AsIs""," +
  @"              ""Capital"": false," +
  @"              ""Cards"": []," +
  @"              ""Children"": null," +
  @"              ""Comments"": null," +
  @"              ""ConditionExpression"": null," +
  @"              ""Enabled"": true," +
  @"              ""Featured"": false," +
  @"              ""Instructions"": null," +
  @"              ""Mandatory"": false," +
  @"              ""PropagationPublicKey"": null," +
  @"              ""PublicKey"": ""b6a042fc-6f87-4d87-8245-d442e2f40347""," +
  @"              ""QuestionText"": ""What is the worst station in the cafeteria?""," +
  @"              ""QuestionType"": ""SingleOption""," +
  @"              ""StataExportCaption"": null," +
  @"              ""Valid"": true," +
  @"              ""ValidationExpression"": ""[a3ce15ab-0745-4ee6-bb19-7e91f691c75a]!=[b6a042fc-6f87-4d87-8245-d442e2f40347]""," +
  @"              ""ValidationMessage"": ""Are you shure? Like and dislike cannot be the same!""," +
  @"              ""Triggers"": []" +
  @"            }" +
  @"          ]," +
  @"          ""ConditionExpression"": """"," +
  @"          ""Enabled"": true," +
  @"          ""Description"": null," +
  @"          ""Propagated"": ""None""," +
  @"          ""PropagationPublicKey"": null," +
  @"          ""PublicKey"": ""e2aaeffe-4b4c-4db1-a773-83bebc394543""," +
  @"          ""Title"": ""CAFETERIA""," +
  @"          ""Triggers"": []" +
  @"        }," +
  @"        {" +
  @"          ""$type"": ""Main.Core.Entities.SubEntities.Complete.CompleteGroup, Main.Core""," +
  @"          ""Children"": [" +
  @"            {" +
  @"              ""$type"": ""Main.Core.Entities.SubEntities.Complete.Question.SingleCompleteQuestion, Main.Core""," +
  @"              ""AddSingleAttr"": null," +
  @"              ""AnswerDate"": ""2012-09-06T19:05:28.0640000""," +
  @"              ""Answers"": [" +
  @"                {" +
  @"                  ""$type"": ""Main.Core.Entities.SubEntities.Complete.CompleteAnswer, Main.Core""," +
  @"                  ""AnswerImage"": """"," +
  @"                  ""AnswerText"": ""Yes""," +
  @"                  ""AnswerType"": ""Select""," +
  @"                  ""AnswerValue"": ""1""," +
  @"                  ""Children"": []," +
  @"                  ""Mandatory"": false," +
  @"                  ""NameCollection"": null," +
  @"                  ""PropogationPublicKey"": null," +
  @"                  ""PublicKey"": ""590bc175-927f-4687-91d1-9eaab73a7f6f""," +
  @"                  ""Selected"": false" +
  @"                }," +
  @"                {" +
  @"                  ""$type"": ""Main.Core.Entities.SubEntities.Complete.CompleteAnswer, Main.Core""," +
  @"                  ""AnswerImage"": """"," +
  @"                  ""AnswerText"": ""No""," +
  @"                  ""AnswerType"": ""Select""," +
  @"                  ""AnswerValue"": ""0""," +
  @"                  ""Children"": []," +
  @"                  ""Mandatory"": false," +
  @"                  ""NameCollection"": null," +
  @"                  ""PropogationPublicKey"": null," +
  @"                  ""PublicKey"": ""d0e08f4d-72fd-407e-a86e-96ad7b2b0463""," +
  @"                  ""Selected"": false" +
  @"                }" +
  @"              ]," +
  @"              ""AnswerOrder"": ""AsIs""," +
  @"              ""Capital"": false," +
  @"              ""Cards"": []," +
  @"              ""Children"": null," +
  @"              ""Comments"": null," +
  @"              ""ConditionExpression"": null," +
  @"              ""Enabled"": true," +
  @"              ""Featured"": false," +
  @"              ""Instructions"": ""The Millennium Cafe offers many delightful treats, salads, sandwiches, and signature drinks.""," +
  @"              ""Mandatory"": false," +
  @"              ""PropagationPublicKey"": null," +
  @"              ""PublicKey"": ""b81ec277-375d-458f-b88d-cfc6af8f20e5""," +
  @"              ""QuestionText"": ""Do you visit millennium cafe?""," +
  @"              ""QuestionType"": ""YesNo""," +
  @"              ""StataExportCaption"": null," +
  @"              ""Valid"": true," +
  @"              ""ValidationExpression"": null," +
  @"              ""ValidationMessage"": null," +
  @"              ""Triggers"": []" +
  @"            }," +
  @"            {" +
  @"              ""$type"": ""Main.Core.Entities.SubEntities.Complete.Question.MultyOptionsCompleteQuestion, Main.Core""," +
  @"              ""AddMultyAttr"": null," +
  @"              ""AnswerDate"": ""2012-09-06T19:05:28.0640000""," +
  @"              ""Answers"": [" +
  @"                {" +
  @"                  ""$type"": ""Main.Core.Entities.SubEntities.Complete.CompleteAnswer, Main.Core""," +
  @"                  ""AnswerImage"": """"," +
  @"                  ""AnswerText"": ""a. Tea""," +
  @"                  ""AnswerType"": ""Select""," +
  @"                  ""AnswerValue"": null," +
  @"                  ""Children"": []," +
  @"                  ""Mandatory"": false," +
  @"                  ""NameCollection"": null," +
  @"                  ""PropogationPublicKey"": null," +
  @"                  ""PublicKey"": ""74e12c58-9030-4de6-b97e-faf50bc181ec""," +
  @"                  ""Selected"": false" +
  @"                }," +
  @"                {" +
  @"                  ""$type"": ""Main.Core.Entities.SubEntities.Complete.CompleteAnswer, Main.Core""," +
  @"                  ""AnswerImage"": """"," +
  @"                  ""AnswerText"": ""b. Coffee""," +
  @"                  ""AnswerType"": ""Select""," +
  @"                  ""AnswerValue"": null," +
  @"                  ""Children"": []," +
  @"                  ""Mandatory"": false," +
  @"                  ""NameCollection"": null," +
  @"                  ""PropogationPublicKey"": null," +
  @"                  ""PublicKey"": ""6632d6f1-ff7a-49db-abe3-661d64fcdda4""," +
  @"                  ""Selected"": false" +
  @"                }," +
  @"                {" +
  @"                  ""$type"": ""Main.Core.Entities.SubEntities.Complete.CompleteAnswer, Main.Core""," +
  @"                  ""AnswerImage"": """"," +
  @"                  ""AnswerText"": ""c. Milk""," +
  @"                  ""AnswerType"": ""Select""," +
  @"                  ""AnswerValue"": null," +
  @"                  ""Children"": []," +
  @"                  ""Mandatory"": false," +
  @"                  ""NameCollection"": null," +
  @"                  ""PropogationPublicKey"": null," +
  @"                  ""PublicKey"": ""edd86f1e-68c5-44bc-9cb2-1056eb8f022e""," +
  @"                  ""Selected"": false" +
  @"                }," +
  @"                {" +
  @"                  ""$type"": ""Main.Core.Entities.SubEntities.Complete.CompleteAnswer, Main.Core""," +
  @"                  ""AnswerImage"": """"," +
  @"                  ""AnswerText"": ""d. Soy Milk""," +
  @"                  ""AnswerType"": ""Select""," +
  @"                  ""AnswerValue"": null," +
  @"                  ""Children"": []," +
  @"                  ""Mandatory"": false," +
  @"                  ""NameCollection"": null," +
  @"                  ""PropogationPublicKey"": null," +
  @"                  ""PublicKey"": ""fe3bf0d6-906b-4ae7-8716-b246f387345a""," +
  @"                  ""Selected"": false" +
  @"                }," +
  @"                {" +
  @"                  ""$type"": ""Main.Core.Entities.SubEntities.Complete.CompleteAnswer, Main.Core""," +
  @"                  ""AnswerImage"": """"," +
  @"                  ""AnswerText"": ""e. Carbonated drinks""," +
  @"                  ""AnswerType"": ""Select""," +
  @"                  ""AnswerValue"": null," +
  @"                  ""Children"": []," +
  @"                  ""Mandatory"": false," +
  @"                  ""NameCollection"": null," +
  @"                  ""PropogationPublicKey"": null," +
  @"                  ""PublicKey"": ""ca73893e-87a8-4936-8b82-a0df98e0516a""," +
  @"                  ""Selected"": false" +
  @"                }" +
  @"              ]," +
  @"              ""AnswerOrder"": ""AsIs""," +
  @"              ""Capital"": false," +
  @"              ""Cards"": []," +
  @"              ""Children"": null," +
  @"              ""Comments"": null," +
  @"              ""ConditionExpression"": ""[b81ec277-375d-458f-b88d-cfc6af8f20e5]=1""," +
  @"              ""Enabled"": true," +
  @"              ""Featured"": false," +
  @"              ""Instructions"": null," +
  @"              ""Mandatory"": false," +
  @"              ""PropagationPublicKey"": null," +
  @"              ""PublicKey"": ""65972bff-9a2a-4667-9932-71440182a2ba""," +
  @"              ""QuestionText"": ""What do you order there?""," +
  @"              ""QuestionType"": ""MultyOption""," +
  @"              ""StataExportCaption"": null," +
  @"              ""Valid"": true," +
  @"              ""ValidationExpression"": null," +
  @"              ""ValidationMessage"": null," +
  @"              ""Triggers"": [" +
  @"                ""b81ec277-375d-458f-b88d-cfc6af8f20e5""," +
  @"                ""b81ec277-375d-458f-b88d-cfc6af8f20e5""," +
  @"                ""b81ec277-375d-458f-b88d-cfc6af8f20e5""," +
  @"                ""b81ec277-375d-458f-b88d-cfc6af8f20e5""," +
  @"                ""b81ec277-375d-458f-b88d-cfc6af8f20e5""," +
  @"                ""b81ec277-375d-458f-b88d-cfc6af8f20e5""," +
  @"                ""b81ec277-375d-458f-b88d-cfc6af8f20e5""" +
  @"              ]" +
  @"            }" +
  @"          ]," +
  @"          ""ConditionExpression"": """"," +
  @"          ""Enabled"": true," +
  @"          ""Description"": null," +
  @"          ""Propagated"": ""None""," +
  @"          ""PropagationPublicKey"": null," +
  @"          ""PublicKey"": ""2c2706ff-3376-41d3-b0bf-53d0a25df178""," +
  @"          ""Title"": ""MILLENNIUM CAFE""," +
  @"          ""Triggers"": []" +
  @"        }," +
  @"        {" +
  @"          ""$type"": ""Main.Core.Entities.SubEntities.Complete.CompleteGroup, Main.Core""," +
  @"          ""Children"": [" +
  @"            {" +
  @"              ""$type"": ""Main.Core.Entities.SubEntities.Complete.Question.MultyOptionsCompleteQuestion, Main.Core""," +
  @"              ""AddMultyAttr"": null," +
  @"              ""AnswerDate"": ""2012-09-06T19:05:28.0640000""," +
  @"              ""Answers"": [" +
  @"                {" +
  @"                  ""$type"": ""Main.Core.Entities.SubEntities.Complete.CompleteAnswer, Main.Core""," +
  @"                  ""AnswerImage"": """"," +
  @"                  ""AnswerText"": ""a. Bus""," +
  @"                  ""AnswerType"": ""Select""," +
  @"                  ""AnswerValue"": null," +
  @"                  ""Children"": []," +
  @"                  ""Mandatory"": false," +
  @"                  ""NameCollection"": null," +
  @"                  ""PropogationPublicKey"": null," +
  @"                  ""PublicKey"": ""d6c3387d-ffbb-4764-877c-0fb437956e71""," +
  @"                  ""Selected"": false" +
  @"                }," +
  @"                {" +
  @"                  ""$type"": ""Main.Core.Entities.SubEntities.Complete.CompleteAnswer, Main.Core""," +
  @"                  ""AnswerImage"": """"," +
  @"                  ""AnswerText"": ""b. Metro""," +
  @"                  ""AnswerType"": ""Select""," +
  @"                  ""AnswerValue"": null," +
  @"                  ""Children"": []," +
  @"                  ""Mandatory"": false," +
  @"                  ""NameCollection"": null," +
  @"                  ""PropogationPublicKey"": null," +
  @"                  ""PublicKey"": ""4f07e489-a972-4a89-a786-62474f08e8c0""," +
  @"                  ""Selected"": false" +
  @"                }," +
  @"                {" +
  @"                  ""$type"": ""Main.Core.Entities.SubEntities.Complete.CompleteAnswer, Main.Core""," +
  @"                  ""AnswerImage"": """"," +
  @"                  ""AnswerText"": ""c. Walk""," +
  @"                  ""AnswerType"": ""Select""," +
  @"                  ""AnswerValue"": null," +
  @"                  ""Children"": []," +
  @"                  ""Mandatory"": false," +
  @"                  ""NameCollection"": null," +
  @"                  ""PropogationPublicKey"": null," +
  @"                  ""PublicKey"": ""5170744e-f62d-4672-8aed-82d72db5d87d""," +
  @"                  ""Selected"": false" +
  @"                }," +
  @"                {" +
  @"                  ""$type"": ""Main.Core.Entities.SubEntities.Complete.CompleteAnswer, Main.Core""," +
  @"                  ""AnswerImage"": """"," +
  @"                  ""AnswerText"": ""d. Car""," +
  @"                  ""AnswerType"": ""Select""," +
  @"                  ""AnswerValue"": null," +
  @"                  ""Children"": []," +
  @"                  ""Mandatory"": false," +
  @"                  ""NameCollection"": null," +
  @"                  ""PropogationPublicKey"": null," +
  @"                  ""PublicKey"": ""bdd54223-4e8f-49d0-8e60-afe5c5e786f3""," +
  @"                  ""Selected"": false" +
  @"                }," +
  @"                {" +
  @"                  ""$type"": ""Main.Core.Entities.SubEntities.Complete.CompleteAnswer, Main.Core""," +
  @"                  ""AnswerImage"": """"," +
  @"                  ""AnswerText"": ""e. Bike""," +
  @"                  ""AnswerType"": ""Select""," +
  @"                  ""AnswerValue"": null," +
  @"                  ""Children"": []," +
  @"                  ""Mandatory"": false," +
  @"                  ""NameCollection"": null," +
  @"                  ""PropogationPublicKey"": null," +
  @"                  ""PublicKey"": ""c29b9260-7ae5-46b1-9506-a21b13e7dc6a""," +
  @"                  ""Selected"": false" +
  @"                }" +
  @"              ]," +
  @"              ""AnswerOrder"": ""AsIs""," +
  @"              ""Capital"": false," +
  @"              ""Cards"": []," +
  @"              ""Children"": null," +
  @"              ""Comments"": null," +
  @"              ""ConditionExpression"": null," +
  @"              ""Enabled"": true," +
  @"              ""Featured"": false," +
  @"              ""Instructions"": null," +
  @"              ""Mandatory"": true," +
  @"              ""PropagationPublicKey"": null," +
  @"              ""PublicKey"": ""c6f45d88-db63-4461-a8a3-07dea5f6849c""," +
  @"              ""QuestionText"": ""How do you go to work?""," +
  @"              ""QuestionType"": ""MultyOption""," +
  @"              ""StataExportCaption"": null," +
  @"              ""Valid"": true," +
  @"              ""ValidationExpression"": null," +
  @"              ""ValidationMessage"": null," +
  @"              ""Triggers"": []" +
  @"            }" +
  @"          ]," +
  @"          ""ConditionExpression"": """"," +
  @"          ""Enabled"": true," +
  @"          ""Description"": null," +
  @"          ""Propagated"": ""None""," +
  @"          ""PropagationPublicKey"": null," +
  @"          ""PublicKey"": ""550226d2-759a-473f-b026-dc6693527819""," +
  @"          ""Title"": ""TRANSPORT""," +
  @"          ""Triggers"": []" +
  @"        }," +
  @"        {" +
  @"          ""$type"": ""Main.Core.Entities.SubEntities.Complete.CompleteGroup, Main.Core""," +
  @"          ""Children"": [" +
  @"            {" +
  @"              ""$type"": ""Main.Core.Entities.SubEntities.Complete.Question.SingleCompleteQuestion, Main.Core""," +
  @"              ""AddSingleAttr"": null," +
  @"              ""AnswerDate"": ""2012-09-06T19:05:28.0640000""," +
  @"              ""Answers"": [" +
  @"                {" +
  @"                  ""$type"": ""Main.Core.Entities.SubEntities.Complete.CompleteAnswer, Main.Core""," +
  @"                  ""AnswerImage"": """"," +
  @"                  ""AnswerText"": ""Yes""," +
  @"                  ""AnswerType"": ""Select""," +
  @"                  ""AnswerValue"": ""1""," +
  @"                  ""Children"": []," +
  @"                  ""Mandatory"": false," +
  @"                  ""NameCollection"": null," +
  @"                  ""PropogationPublicKey"": null," +
  @"                  ""PublicKey"": ""e42207c3-1bcd-4de6-8fd1-4ad9b6d9991f""," +
  @"                  ""Selected"": false" +
  @"                }," +
  @"                {" +
  @"                  ""$type"": ""Main.Core.Entities.SubEntities.Complete.CompleteAnswer, Main.Core""," +
  @"                  ""AnswerImage"": """"," +
  @"                  ""AnswerText"": ""No""," +
  @"                  ""AnswerType"": ""Select""," +
  @"                  ""AnswerValue"": ""0""," +
  @"                  ""Children"": []," +
  @"                  ""Mandatory"": false," +
  @"                  ""NameCollection"": null," +
  @"                  ""PropogationPublicKey"": null," +
  @"                  ""PublicKey"": ""147f3ff3-f813-4c8a-89ab-33610e946fa6""," +
  @"                  ""Selected"": false" +
  @"                }" +
  @"              ]," +
  @"              ""AnswerOrder"": ""AsIs""," +
  @"              ""Capital"": false," +
  @"              ""Cards"": []," +
  @"              ""Children"": null," +
  @"              ""Comments"": null," +
  @"              ""ConditionExpression"": null," +
  @"              ""Enabled"": true," +
  @"              ""Featured"": false," +
  @"              ""Instructions"": null," +
  @"              ""Mandatory"": false," +
  @"              ""PropagationPublicKey"": null," +
  @"              ""PublicKey"": ""8c17f588-9e48-4115-bf52-474b6f429b02""," +
  @"              ""QuestionText"": ""Do you own any Apple products?""," +
  @"              ""QuestionType"": ""YesNo""," +
  @"              ""StataExportCaption"": null," +
  @"              ""Valid"": true," +
  @"              ""ValidationExpression"": null," +
  @"              ""ValidationMessage"": null," +
  @"              ""Triggers"": []" +
  @"            }," +
  @"            {" +
  @"              ""$type"": ""Main.Core.Entities.SubEntities.Complete.Question.MultyOptionsCompleteQuestion, Main.Core""," +
  @"              ""AddMultyAttr"": null," +
  @"              ""AnswerDate"": ""2012-09-06T19:05:28.0640000""," +
  @"              ""Answers"": [" +
  @"                {" +
  @"                  ""$type"": ""Main.Core.Entities.SubEntities.Complete.CompleteAnswer, Main.Core""," +
  @"                  ""AnswerImage"": ""45ccb0e6-fc33-4575-b961-7a309c1b28e7""," +
  @"                  ""AnswerText"": ""a. IMac ""," +
  @"                  ""AnswerType"": ""Image""," +
  @"                  ""AnswerValue"": null," +
  @"                  ""Children"": []," +
  @"                  ""Mandatory"": false," +
  @"                  ""NameCollection"": null," +
  @"                  ""PropogationPublicKey"": null," +
  @"                  ""PublicKey"": ""00c7906a-0e6c-4149-bea3-737e69677d1a""," +
  @"                  ""Selected"": false" +
  @"                }," +
  @"                {" +
  @"                  ""$type"": ""Main.Core.Entities.SubEntities.Complete.CompleteAnswer, Main.Core""," +
  @"                  ""AnswerImage"": ""5ac803f5-6983-4b3e-8f92-0e8730daa62c""," +
  @"                  ""AnswerText"": ""b. IPAD""," +
  @"                  ""AnswerType"": ""Image""," +
  @"                  ""AnswerValue"": null," +
  @"                  ""Children"": []," +
  @"                  ""Mandatory"": false," +
  @"                  ""NameCollection"": null," +
  @"                  ""PropogationPublicKey"": null," +
  @"                  ""PublicKey"": ""2ba21ed5-b692-4053-8744-a63c87ef793b""," +
  @"                  ""Selected"": false" +
  @"                }," +
  @"                {" +
  @"                  ""$type"": ""Main.Core.Entities.SubEntities.Complete.CompleteAnswer, Main.Core""," +
  @"                  ""AnswerImage"": ""a9734df3-c1d3-4d01-9644-80539f243022""," +
  @"                  ""AnswerText"": ""c. IPOD""," +
  @"                  ""AnswerType"": ""Image""," +
  @"                  ""AnswerValue"": null," +
  @"                  ""Children"": []," +
  @"                  ""Mandatory"": false," +
  @"                  ""NameCollection"": null," +
  @"                  ""PropogationPublicKey"": null," +
  @"                  ""PublicKey"": ""6e30fa97-d7e0-4047-95e9-9e0e07ac60ba""," +
  @"                  ""Selected"": false" +
  @"                }," +
  @"                {" +
  @"                  ""$type"": ""Main.Core.Entities.SubEntities.Complete.CompleteAnswer, Main.Core""," +
  @"                  ""AnswerImage"": ""cad897b1-f1a2-472e-9b3a-ef644ddb2545""," +
  @"                  ""AnswerText"": ""d. IPHONE""," +
  @"                  ""AnswerType"": ""Image""," +
  @"                  ""AnswerValue"": null," +
  @"                  ""Children"": []," +
  @"                  ""Mandatory"": false," +
  @"                  ""NameCollection"": null," +
  @"                  ""PropogationPublicKey"": null," +
  @"                  ""PublicKey"": ""3cef7244-7fce-489b-8d1a-a3bf17713529""," +
  @"                  ""Selected"": false" +
  @"                }" +
  @"              ]," +
  @"              ""AnswerOrder"": ""AsIs""," +
  @"              ""Capital"": false," +
  @"              ""Cards"": []," +
  @"              ""Children"": null," +
  @"              ""Comments"": null," +
  @"              ""ConditionExpression"": ""[8c17f588-9e48-4115-bf52-474b6f429b02]=1""," +
  @"              ""Enabled"": true," +
  @"              ""Featured"": false," +
  @"              ""Instructions"": null," +
  @"              ""Mandatory"": false," +
  @"              ""PropagationPublicKey"": null," +
  @"              ""PublicKey"": ""e7f79783-209c-4f63-9f80-7b6c63dc9298""," +
  @"              ""QuestionText"": ""What are they?""," +
  @"              ""QuestionType"": ""MultyOption""," +
  @"              ""StataExportCaption"": null," +
  @"              ""Valid"": true," +
  @"              ""ValidationExpression"": null," +
  @"              ""ValidationMessage"": null," +
  @"              ""Triggers"": [" +
  @"                ""8c17f588-9e48-4115-bf52-474b6f429b02""," +
  @"                ""8c17f588-9e48-4115-bf52-474b6f429b02""," +
  @"                ""8c17f588-9e48-4115-bf52-474b6f429b02""," +
  @"                ""8c17f588-9e48-4115-bf52-474b6f429b02""," +
  @"                ""8c17f588-9e48-4115-bf52-474b6f429b02""," +
  @"                ""8c17f588-9e48-4115-bf52-474b6f429b02""," +
  @"                ""8c17f588-9e48-4115-bf52-474b6f429b02""" +
  @"              ]" +
  @"            }" +
  @"          ]," +
  @"          ""ConditionExpression"": """"," +
  @"          ""Enabled"": true," +
  @"          ""Description"": null," +
  @"          ""Propagated"": ""None""," +
  @"          ""PropagationPublicKey"": null," +
  @"          ""PublicKey"": ""e7fb1f6c-cbe5-4083-936c-8ef77d0e832f""," +
  @"          ""Title"": ""APPLE PRODUCTS""," +
  @"          ""Triggers"": []" +
  @"        }," +
  @"        {" +
  @"          ""$type"": ""Main.Core.Entities.SubEntities.Complete.CompleteGroup, Main.Core""," +
  @"          ""Children"": [" +
  @"            {" +
  @"              ""$type"": ""Main.Core.Entities.SubEntities.Complete.Question.SingleCompleteQuestion, Main.Core""," +
  @"              ""AddSingleAttr"": null," +
  @"              ""AnswerDate"": ""2012-09-06T19:05:28.0640000""," +
  @"              ""Answers"": [" +
  @"                {" +
  @"                  ""$type"": ""Main.Core.Entities.SubEntities.Complete.CompleteAnswer, Main.Core""," +
  @"                  ""AnswerImage"": """"," +
  @"                  ""AnswerText"": ""Rent""," +
  @"                  ""AnswerType"": ""Select""," +
  @"                  ""AnswerValue"": ""1""," +
  @"                  ""Children"": []," +
  @"                  ""Mandatory"": false," +
  @"                  ""NameCollection"": null," +
  @"                  ""PropogationPublicKey"": null," +
  @"                  ""PublicKey"": ""91ad3cd0-7298-4b29-a4cc-246ed17eca0a""," +
  @"                  ""Selected"": false" +
  @"                }," +
  @"                {" +
  @"                  ""$type"": ""Main.Core.Entities.SubEntities.Complete.CompleteAnswer, Main.Core""," +
  @"                  ""AnswerImage"": """"," +
  @"                  ""AnswerText"": ""Own""," +
  @"                  ""AnswerType"": ""Select""," +
  @"                  ""AnswerValue"": ""0""," +
  @"                  ""Children"": []," +
  @"                  ""Mandatory"": false," +
  @"                  ""NameCollection"": null," +
  @"                  ""PropogationPublicKey"": null," +
  @"                  ""PublicKey"": ""f461410b-66e1-41cd-baf7-192b30baa99b""," +
  @"                  ""Selected"": false" +
  @"                }," +
  @"                {" +
  @"                  ""$type"": ""Main.Core.Entities.SubEntities.Complete.CompleteAnswer, Main.Core""," +
  @"                  ""AnswerImage"": """"," +
  @"                  ""AnswerText"": ""Other""," +
  @"                  ""AnswerType"": ""Select""," +
  @"                  ""AnswerValue"": ""2""," +
  @"                  ""Children"": []," +
  @"                  ""Mandatory"": false," +
  @"                  ""NameCollection"": null," +
  @"                  ""PropogationPublicKey"": null," +
  @"                  ""PublicKey"": ""e2d12972-d59e-4733-8381-1c6ada288cf0""," +
  @"                  ""Selected"": false" +
  @"                }" +
  @"              ]," +
  @"              ""AnswerOrder"": ""AsIs""," +
  @"              ""Capital"": false," +
  @"              ""Cards"": []," +
  @"              ""Children"": null," +
  @"              ""Comments"": null," +
  @"              ""ConditionExpression"": null," +
  @"              ""Enabled"": true," +
  @"              ""Featured"": false," +
  @"              ""Instructions"": null," +
  @"              ""Mandatory"": false," +
  @"              ""PropagationPublicKey"": null," +
  @"              ""PublicKey"": ""a9a7a6e3-ea53-41e9-8795-a7edb36ddb5d""," +
  @"              ""QuestionText"": ""Do you rent or own your house?""," +
  @"              ""QuestionType"": ""SingleOption""," +
  @"              ""StataExportCaption"": null," +
  @"              ""Valid"": true," +
  @"              ""ValidationExpression"": null," +
  @"              ""ValidationMessage"": null," +
  @"              ""Triggers"": []" +
  @"            }," +
  @"            {" +
  @"              ""$type"": ""Main.Core.Entities.SubEntities.Complete.Question.TextCompleteQuestion, Main.Core""," +
  @"              ""AddTextAttr"": null," +
  @"              ""Answer"": null," +
  @"              ""AnswerDate"": ""2012-09-06T19:05:28.0640000""," +
  @"              ""Answers"": []," +
  @"              ""AnswerOrder"": ""AsIs""," +
  @"              ""Capital"": false," +
  @"              ""Cards"": []," +
  @"              ""Children"": null," +
  @"              ""Comments"": null," +
  @"              ""ConditionExpression"": ""[a9a7a6e3-ea53-41e9-8795-a7edb36ddb5d]=2""," +
  @"              ""Enabled"": true," +
  @"              ""Featured"": false," +
  @"              ""Instructions"": null," +
  @"              ""Mandatory"": false," +
  @"              ""PropagationPublicKey"": null," +
  @"              ""PublicKey"": ""8364fecf-35e6-4a77-9049-2e5ed63f8f5d""," +
  @"              ""QuestionText"": ""If other specify""," +
  @"              ""QuestionType"": ""Text""," +
  @"              ""StataExportCaption"": null," +
  @"              ""Valid"": true," +
  @"              ""ValidationExpression"": null," +
  @"              ""ValidationMessage"": null," +
  @"              ""Triggers"": [" +
  @"                ""a9a7a6e3-ea53-41e9-8795-a7edb36ddb5d""" +
  @"              ]" +
  @"            }," +
  @"            {" +
  @"              ""$type"": ""Main.Core.Entities.SubEntities.Complete.Question.AutoPropagateCompleteQuestion, Main.Core""," +
  @"              ""TargetGroupKey"": ""7995abad-41a3-45ac-bc33-40745674e998""," +
  @"              ""Answer"": null," +
  @"              ""MaxValue"": 0," +
  @"              ""AnswerDate"": ""2012-09-06T19:05:28.0640000""," +
  @"              ""Answers"": []," +
  @"              ""AnswerOrder"": ""AsIs""," +
  @"              ""Capital"": false," +
  @"              ""Cards"": []," +
  @"              ""Children"": null," +
  @"              ""Comments"": null," +
  @"              ""ConditionExpression"": null," +
  @"              ""Enabled"": true," +
  @"              ""Featured"": false," +
  @"              ""Instructions"": null," +
  @"              ""Mandatory"": false," +
  @"              ""PropagationPublicKey"": null," +
  @"              ""PublicKey"": ""9a717e52-cf9f-4964-bd7f-505e4a9e7e4e""," +
  @"              ""QuestionText"": ""How many rooms do you have?""," +
  @"              ""QuestionType"": ""AutoPropagate""," +
  @"              ""StataExportCaption"": ""dgdfg""," +
  @"              ""Valid"": true," +
  @"              ""ValidationExpression"": null," +
  @"              ""ValidationMessage"": null," +
  @"              ""Triggers"": [" +
  @"                ""7995abad-41a3-45ac-bc33-40745674e998""" +
  @"              ]" +
  @"            }" +
  @"          ]," +
  @"          ""ConditionExpression"": """"," +
  @"          ""Enabled"": true," +
  @"          ""Description"": null," +
  @"          ""Propagated"": ""None""," +
  @"          ""PropagationPublicKey"": null," +
  @"          ""PublicKey"": ""2e070a63-1908-4f11-b816-4ea87c1ab35c""," +
  @"          ""Title"": ""HOUSE""," +
  @"          ""Triggers"": []" +
  @"        }," +
  @"        {" +
  @"          ""$type"": ""Main.Core.Entities.SubEntities.Complete.CompleteGroup, Main.Core""," +
  @"          ""Children"": [" +
  @"            {" +
  @"              ""$type"": ""Main.Core.Entities.SubEntities.Complete.CompleteGroup, Main.Core""," +
  @"              ""Children"": [" +
  @"                {" +
  @"                  ""$type"": ""Main.Core.Entities.SubEntities.Complete.Question.NumericCompleteQuestion, Main.Core""," +
  @"                  ""AddNumericAttr"": null," +
  @"                  ""IntAttr"": 0," +
  @"                  ""Answer"": null," +
  @"                  ""AnswerDate"": ""2012-09-06T19:05:28.0640000""," +
  @"                  ""Answers"": []," +
  @"                  ""AnswerOrder"": ""AsIs""," +
  @"                  ""Capital"": false," +
  @"                  ""Cards"": []," +
  @"                  ""Children"": null," +
  @"                  ""Comments"": null," +
  @"                  ""ConditionExpression"": null," +
  @"                  ""Enabled"": true," +
  @"                  ""Featured"": false," +
  @"                  ""Instructions"": null," +
  @"                  ""Mandatory"": false," +
  @"                  ""PropagationPublicKey"": null," +
  @"                  ""PublicKey"": ""b4817432-1e18-4437-bc4c-22a524fb1e71""," +
  @"                  ""QuestionText"": ""What is the area of room?""," +
  @"                  ""QuestionType"": ""Numeric""," +
  @"                  ""StataExportCaption"": null," +
  @"                  ""Valid"": true," +
  @"                  ""ValidationExpression"": null," +
  @"                  ""ValidationMessage"": null," +
  @"                  ""Triggers"": []" +
  @"                }," +
  @"                {" +
  @"                  ""$type"": ""Main.Core.Entities.SubEntities.Complete.Question.SingleCompleteQuestion, Main.Core""," +
  @"                  ""AddSingleAttr"": null," +
  @"                  ""AnswerDate"": ""2012-09-06T19:05:28.0640000""," +
  @"                  ""Answers"": [" +
  @"                    {" +
  @"                      ""$type"": ""Main.Core.Entities.SubEntities.Complete.CompleteAnswer, Main.Core""," +
  @"                      ""AnswerImage"": null," +
  @"                      ""AnswerText"": ""Living room""," +
  @"                      ""AnswerType"": ""Select""," +
  @"                      ""AnswerValue"": ""0""," +
  @"                      ""Children"": []," +
  @"                      ""Mandatory"": false," +
  @"                      ""NameCollection"": null," +
  @"                      ""PropogationPublicKey"": null," +
  @"                      ""PublicKey"": ""c826dad9-d76a-49d6-a40a-590fc0265b74""," +
  @"                      ""Selected"": false" +
  @"                    }," +
  @"                    {" +
  @"                      ""$type"": ""Main.Core.Entities.SubEntities.Complete.CompleteAnswer, Main.Core""," +
  @"                      ""AnswerImage"": null," +
  @"                      ""AnswerText"": ""Kitchen""," +
  @"                      ""AnswerType"": ""Select""," +
  @"                      ""AnswerValue"": ""1""," +
  @"                      ""Children"": []," +
  @"                      ""Mandatory"": false," +
  @"                      ""NameCollection"": null," +
  @"                      ""PropogationPublicKey"": null," +
  @"                      ""PublicKey"": ""737d0ec8-850b-4138-b239-8f5a1d0da874""," +
  @"                      ""Selected"": false" +
  @"                    }," +
  @"                    {" +
  @"                      ""$type"": ""Main.Core.Entities.SubEntities.Complete.CompleteAnswer, Main.Core""," +
  @"                      ""AnswerImage"": null," +
  @"                      ""AnswerText"": ""Bedroom""," +
  @"                      ""AnswerType"": ""Select""," +
  @"                      ""AnswerValue"": ""2""," +
  @"                      ""Children"": []," +
  @"                      ""Mandatory"": false," +
  @"                      ""NameCollection"": null," +
  @"                      ""PropogationPublicKey"": null," +
  @"                      ""PublicKey"": ""6f4f93b2-0e16-4a2f-8606-68465c383d0f""," +
  @"                      ""Selected"": false" +
  @"                    }," +
  @"                    {" +
  @"                      ""$type"": ""Main.Core.Entities.SubEntities.Complete.CompleteAnswer, Main.Core""," +
  @"                      ""AnswerImage"": null," +
  @"                      ""AnswerText"": ""Cabinet""," +
  @"                      ""AnswerType"": ""Select""," +
  @"                      ""AnswerValue"": ""3""," +
  @"                      ""Children"": []," +
  @"                      ""Mandatory"": false," +
  @"                      ""NameCollection"": null," +
  @"                      ""PropogationPublicKey"": null," +
  @"                      ""PublicKey"": ""9df57979-6578-4883-846e-6675333b2f9e""," +
  @"                      ""Selected"": false" +
  @"                    }" +
  @"                  ]," +
  @"                  ""AnswerOrder"": ""AsIs""," +
  @"                  ""Capital"": true," +
  @"                  ""Cards"": []," +
  @"                  ""Children"": null," +
  @"                  ""Comments"": null," +
  @"                  ""ConditionExpression"": null," +
  @"                  ""Enabled"": true," +
  @"                  ""Featured"": false," +
  @"                  ""Instructions"": null," +
  @"                  ""Mandatory"": false," +
  @"                  ""PropagationPublicKey"": null," +
  @"                  ""PublicKey"": ""a61498b9-d0f2-4b11-a372-db6ed7ef6098""," +
  @"                  ""QuestionText"": ""Room type""," +
  @"                  ""QuestionType"": ""SingleOption""," +
  @"                  ""StataExportCaption"": ""room_type""," +
  @"                  ""Valid"": true," +
  @"                  ""ValidationExpression"": null," +
  @"                  ""ValidationMessage"": null," +
  @"                  ""Triggers"": []" +
  @"                }" +
  @"              ]," +
  @"              ""ConditionExpression"": """"," +
  @"              ""Enabled"": true," +
  @"              ""Description"": null," +
  @"              ""Propagated"": ""AutoPropagated""," +
  @"              ""PropagationPublicKey"": null," +
  @"              ""PublicKey"": ""7995abad-41a3-45ac-bc33-40745674e998""," +
  @"              ""Title"": ""Description of rooms""," +
  @"              ""Triggers"": []" +
  @"            }" +
  @"          ]," +
  @"          ""ConditionExpression"": """"," +
  @"          ""Enabled"": true," +
  @"          ""Description"": null," +
  @"          ""Propagated"": ""None""," +
  @"          ""PropagationPublicKey"": null," +
  @"          ""PublicKey"": ""0ac2b813-e392-4462-a036-497fc3975a48""," +
  @"          ""Title"": ""HOUSE DETAILS""," +
  @"          ""Triggers"": []" +
  @"        }" +
  @"      ]," +
  @"      ""CloseDate"": null," +
  @"      ""ConditionExpression"": """"," +
  @"      ""CreationDate"": ""2012-09-06T19:05:28.0640000""," +
  @"      ""Creator"": null," +
  @"      ""Enabled"": true," +
  @"      ""LastEntryDate"": ""2012-09-06T19:05:28.0640000""," +
  @"      ""Description"": null," +
  @"      ""LastVisitedGroup"": null," +
  @"      ""OpenDate"": null," +
  @"      ""Propagated"": ""None""," +
  @"      ""PropagationPublicKey"": null," +
  @"      ""PublicKey"": ""9a9be11f-ccdb-4dfb-9705-4bae81153363""," +
  @"      ""Responsible"": null," +
  @"      ""Status"": {" +
  @"        ""ChangeComment"": null," +
  @"        ""Name"": ""Initial""," +
  @"        ""PublicId"": ""8927d124-3cfb-4374-ad36-2fd99b62ce13""" +
  @"      }," +
  @"      ""StatusChangeComment"": null," +
  @"      ""TemplateId"": ""2213d3cb-bf96-4c5f-813d-438759066c55""," +
  @"      ""Title"": ""2012 Research department general survey""," +
  @"      ""Triggers"": []" +
  @"    }";
            root = JsonConvert.DeserializeObject<CompleteQuestionnaireDocument>(s, settings);

        }

        #region Implementation of IViewFactory<QuestionnaireScreenInput,QuestionnaireScreenViewModel>

        public QuestionnaireScreenViewModel Load(QuestionnaireScreenInput input)
        {
            ICompleteGroup screen = null;
            IList<QuestionnaireNavigationPanelItem> siblings = new List<QuestionnaireNavigationPanelItem>(0);
            if (!input.ScreenPublicKey.HasValue)
            {
                screen = root.Children.OfType<ICompleteGroup>().First();
                siblings =
                    root.Children.OfType<ICompleteGroup>().Select(
                        g => new QuestionnaireNavigationPanelItem(g.PublicKey, g.Title, 0, 0)).ToList();
            }
            else
            {
                Queue<ICompleteGroup> groups = new Queue<ICompleteGroup>(new ICompleteGroup[] {root});
                while (groups.Count > 0)
                {
                    var current = groups.Dequeue();
                    var possibleScreen =
                        current.Children.OfType<ICompleteGroup>().FirstOrDefault(
                            g => g.PublicKey == input.ScreenPublicKey);
                    if (possibleScreen != null)
                    {
                        screen = possibleScreen;
                        siblings =
                            current.Children.OfType<ICompleteGroup>().Select(
                                g => new QuestionnaireNavigationPanelItem(g.PublicKey, g.Title, 0, 0)).ToList();
                        break;
                    }
                    foreach (ICompleteGroup completeGroup in current.Children.OfType<ICompleteGroup>())
                    {
                        groups.Enqueue(completeGroup);
                    }

                }

            }
            if (screen == null)
                throw new ArgumentException("screen cant be found");
            return new QuestionnaireScreenViewModel(input.QuestionnaireId, screen.PublicKey,
                                                    screen.PropagationPublicKey, BuildItems(screen), siblings,
                                                    Enumerable.Empty<QuestionnaireNavigationPanelItem>(),
                                                    BuildChapters(root));
        }

        #endregion
        protected IEnumerable<QuestionnaireNavigationPanelItem> BuildChapters(CompleteQuestionnaireDocument root)
        {
            return
                root.Children.OfType<ICompleteGroup>().Select(
                    g => new QuestionnaireNavigationPanelItem(g.PublicKey, g.Title, 0, 0));
        }
        protected IEnumerable<IQuestionnaireItemViewModel> BuildItems(ICompleteGroup screen)
        {
            return screen.Children.Select(CreateView);
        }
        protected IQuestionnaireItemViewModel CreateView(IComposite item)
        {
            var question = item as ICompleteQuestion;
            if (question != null)
                if (question.QuestionType == QuestionType.AutoPropagate ||
                    question.QuestionType == QuestionType.DateTime ||
                    question.QuestionType == QuestionType.GpsCoordinates ||
                    question.QuestionType == QuestionType.Numeric ||
                    question.QuestionType == QuestionType.Percentage ||
                    question.QuestionType == QuestionType.Text)
                    return new ValueQuestionViewModel(question.PublicKey, question.QuestionText,
                                                           CalculateViewType(question.QuestionType), question.GetAnswerString(),
                                                           question.Enabled, question.Instructions, question.Comments,
                                                           question.Valid, question.Mandatory);
                else
                    return new SelectebleQuestionViewModel(question.PublicKey, question.QuestionText,
                                                          CalculateViewType(question.QuestionType),
                                                           question.Answers.OfType<ICompleteAnswer>().Select(
                                                               a =>
                                                               new AnswerViewModel(a.PublicKey, a.AnswerText, a.Selected)),
                                                           question.Enabled, question.Instructions, question.Comments,
                                                           question.Valid, question.Mandatory);
            var group = item as ICompleteGroup;
            if (group != null)
                return new GroupViewModel(group.PublicKey, group.Title, group.Enabled);
            return null;
        }
        protected QuestionType CalculateViewType(QuestionType type)
        {
            if (type == QuestionType.Numeric || type == QuestionType.AutoPropagate)
                return QuestionType.Numeric;
            if (type == QuestionType.Text || type == QuestionType.Percentage || type == QuestionType.Text)
                return QuestionType.Text;
            if (type == QuestionType.SingleOption || type == QuestionType.DropDownList || type == QuestionType.YesNo)
                return QuestionType.SingleOption;
            return type;
        }
    }
}
