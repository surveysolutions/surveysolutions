
function supports_html5_storage() {
    try {
        return 'localStorage' in window && window['localStorage'] !== null;
    } catch (e) {
        return false;
    }
}
function supports_json() {
    try {
        return 'JSON' in window && window['JSON'] !== null;
    } catch (e) {
        return false;
    }
}

$(document).ready(function () {
    //Adding to storage
    function addToStorage(id, label) {
        if (!hasInStorage(id)) {
            var data = getStorage();
            data[id] = label;
            saveStorage(data);
        }
    }

    //loading from storage
    function getStorage() {
        var current = localStorage["favorites"];
        var data = {};
        if (typeof current != "undefined" && current) data = window.JSON.parse(current);
        return data;
    }

    //Checking storage
    function hasInStorage(id) {
        return (id in getStorage());
    }
    //Checking storage
    function getFromStorage(id) {
        if (!hasInStorage(id))
            return null;
        return getStorage()[id];
    }
    //Adding to storage
    function removeFromStorage(id, label) {
        if (hasInStorage(id)) {
            var data = getStorage();
            delete data[id];
            console.log('removed ' + id);
            saveStorage(data);
        }
    }

    //save storage
    function saveStorage(data) {
        console.log("To store...");
        console.dir(data);
        localStorage["favorites"] = window.JSON.stringify(data);
    }

    //only bother if we support storage
    if (supports_html5_storage() && supports_json()) {
        var qId = "completequestionnairedocuments/145409";
        if (!hasInStorage(qId))
            addToStorage(qId, $.parseJSON('{    "Creator": {      "Id": "16385",      "Name": "admin"   },   "TemplateId": "questionnairedocuments/5121",   "Status": {     "Id": "5121",     "Name": "Initial"   },   "Responsible": {     "Id": "16385",     "Name": "admin"   },   "StatusChangeComment": null,   "Questions": [     {       "$type": "RavenQuestionnaire.Core.Entities.SubEntities.Complete.CompleteQuestion, RavenQuestionnaire.Core",       "PublicKey": "bff43236-6a03-4977-99a4-8abe2f8bed98",       "Title": "1",       "QuestionType": "Numeric",       "Answers": [],       "ConditionExpression": null,       "StataExportCaption": "eeee",       "Instructions": null,       "Enabled": true,       "Triggers": []     },     {       "$type": "RavenQuestionnaire.Core.Entities.SubEntities.Complete.CompleteQuestion, RavenQuestionnaire.Core",       "PublicKey": "bd5bc8c0-62ab-433b-8b6c-4446b5f80f75",       "Title": "2",       "QuestionType": "Text",       "Answers": [],       "ConditionExpression": "",       "StataExportCaption": "2",       "Instructions": null,       "Enabled": true,       "Triggers": []     },     {       "$type": "RavenQuestionnaire.Core.Entities.SubEntities.Complete.CompleteQuestion, RavenQuestionnaire.Core",       "PublicKey": "e7fe283c-56f9-4879-8e23-034fd3da8f66",       "Title": "ffghfghfgh",       "QuestionType": "SingleOption",       "Answers": [         {           "$type": "RavenQuestionnaire.Core.Entities.SubEntities.Complete.CompleteAnswer, RavenQuestionnaire.Core",           "PublicKey": "7071f5db-8e16-4154-a5a5-88f2a728b3fc",           "Title": "fdg1",           "Mandatory": false,           "AnswerType": "Select",           "AnswerValue": "1",           "QuestionPublicKey": "e7fe283c-56f9-4879-8e23-034fd3da8f66",           "Selected": false         },         {           "$type": "RavenQuestionnaire.Core.Entities.SubEntities.Complete.CompleteAnswer, RavenQuestionnaire.Core",           "PublicKey": "0ae248ee-0bcd-4e51-941d-f4990a0ee73e",           "Title": "ewr4",           "Mandatory": false,           "AnswerType": "Select",           "AnswerValue": "2",           "QuestionPublicKey": "e7fe283c-56f9-4879-8e23-034fd3da8f66",           "Selected": false         }       ],       "ConditionExpression": null,       "StataExportCaption": "56",       "Instructions": null,       "Enabled": true,       "Triggers": []     },     {       "$type": "RavenQuestionnaire.Core.Entities.SubEntities.Complete.CompleteQuestion, RavenQuestionnaire.Core",       "PublicKey": "2ee031fb-72b9-4ef2-a9be-bf2ffa2feb76",       "Title": "ffff",       "QuestionType": "DateTime",       "Answers": [],       "ConditionExpression": null,       "StataExportCaption": null,       "Instructions": null,       "Enabled": true,       "Triggers": []     },     {       "$type": "RavenQuestionnaire.Core.Entities.SubEntities.Complete.CompleteQuestion, RavenQuestionnaire.Core",       "PublicKey": "37d84f79-ed3a-4208-8f37-0214903c4058",       "Title": "multy",       "QuestionType": "MultyOption",       "Answers": [         {           "$type": "RavenQuestionnaire.Core.Entities.SubEntities.Complete.CompleteAnswer, RavenQuestionnaire.Core",           "PublicKey": "5311a986-51a3-4550-be65-2f592b7567a5",           "Title": "fdg",           "Mandatory": false,           "AnswerType": "Select",           "AnswerValue": "3",           "QuestionPublicKey": "37d84f79-ed3a-4208-8f37-0214903c4058",           "Selected": false         },         {           "$type": "RavenQuestionnaire.Core.Entities.SubEntities.Complete.CompleteAnswer, RavenQuestionnaire.Core",           "PublicKey": "8a355667-0b94-4b5f-b37a-a385c81c6d9e",           "Title": "dfgdfg",           "Mandatory": false,           "AnswerType": "Select",           "AnswerValue": "2",           "QuestionPublicKey": "37d84f79-ed3a-4208-8f37-0214903c4058",           "Selected": false         },         {           "$type": "RavenQuestionnaire.Core.Entities.SubEntities.Complete.CompleteAnswer, RavenQuestionnaire.Core",           "PublicKey": "8316959c-d027-44f4-9610-13c5a66b84b4",           "Title": "dfgdfd",           "Mandatory": false,           "AnswerType": "Select",           "AnswerValue": "8",           "QuestionPublicKey": "37d84f79-ed3a-4208-8f37-0214903c4058",           "Selected": false         }       ],       "ConditionExpression": null,       "StataExportCaption": "d",       "Instructions": null,       "Enabled": true,       "Triggers": []     }   ],   "Groups": [     {       "$type": "RavenQuestionnaire.Core.Entities.SubEntities.Complete.CompleteGroup, RavenQuestionnaire.Core",       "PublicKey": "c3462a5d-9946-40b5-9eb1-049df880c83b",       "Title": "fgh",       "Propagated": "None",       "Triggers": [],       "Questions": [         {           "$type": "RavenQuestionnaire.Core.Entities.SubEntities.Complete.BindedCompleteQuestion, RavenQuestionnaire.Core",           "PublicKey": "c7a111f3-8b75-4b4d-ab00-e37d63314cb6",           "Title": null,           "QuestionType": "SingleOption",           "ConditionExpression": null,           "StataExportCaption": null,           "Instructions": null,           "Enabled": false,           "Answers": [],           "ParentPublicKey": "bd5bc8c0-62ab-433b-8b6c-4446b5f80f75"         },         {           "$type": "RavenQuestionnaire.Core.Entities.SubEntities.Complete.CompleteQuestion, RavenQuestionnaire.Core",           "PublicKey": "e027817c-065c-4c0e-ae65-cdff1acfc040",           "Title": "fghfgh",           "QuestionType": "SingleOption",           "Answers": [             {               "$type": "RavenQuestionnaire.Core.Entities.SubEntities.Complete.CompleteAnswer, RavenQuestionnaire.Core",               "PublicKey": "8fcc101e-6224-4c6c-b68b-93e91ddfa80f",               "Title": "1w",               "Mandatory": false,               "AnswerType": "Select",               "AnswerValue": "1",               "QuestionPublicKey": "e027817c-065c-4c0e-ae65-cdff1acfc040",               "Selected": false             },             {               "$type": "RavenQuestionnaire.Core.Entities.SubEntities.Complete.CompleteAnswer, RavenQuestionnaire.Core",               "PublicKey": "99dda845-8c63-4ae9-a032-6a350eb5196b",               "Title": "23",               "Mandatory": false,               "AnswerType": "Select",               "AnswerValue": "2",               "QuestionPublicKey": "e027817c-065c-4c0e-ae65-cdff1acfc040",               "Selected": false             },             {               "$type": "RavenQuestionnaire.Core.Entities.SubEntities.Complete.CompleteAnswer, RavenQuestionnaire.Core",               "PublicKey": "f6dfc2a1-9501-443e-a2dd-36f9f10681c6",               "Title": "3ert",               "Mandatory": false,               "AnswerType": "Text",               "AnswerValue": "3",               "QuestionPublicKey": "e027817c-065c-4c0e-ae65-cdff1acfc040",               "Selected": false             }           ],           "ConditionExpression": "[bff43236-6a03-4977-99a4-8abe2f8bed98]>3",           "StataExportCaption": "q3",           "Instructions": null,           "Enabled": true,           "Triggers": [             "bff43236-6a03-4977-99a4-8abe2f8bed98"           ]         },         {           "$type": "RavenQuestionnaire.Core.Entities.SubEntities.Complete.CompleteQuestion, RavenQuestionnaire.Core",           "PublicKey": "01623724-c9b0-45d5-8e7b-c8a0a72ccfd1",           "Title": "ghj",           "QuestionType": "DropDownList",           "Answers": [             {               "$type": "RavenQuestionnaire.Core.Entities.SubEntities.Complete.CompleteAnswer, RavenQuestionnaire.Core",               "PublicKey": "8b2b2d32-b173-4e56-81ef-c1bbf28821e0",               "Title": "ggghj",               "Mandatory": false,               "AnswerType": "Select",               "AnswerValue": "1",               "QuestionPublicKey": "01623724-c9b0-45d5-8e7b-c8a0a72ccfd1",               "Selected": false             },             {               "$type": "RavenQuestionnaire.Core.Entities.SubEntities.Complete.CompleteAnswer, RavenQuestionnaire.Core",               "PublicKey": "3907d722-78ee-47d8-9764-a243534a31e4",               "Title": "i98kl;",               "Mandatory": false,               "AnswerType": "Select",               "AnswerValue": "2",               "QuestionPublicKey": "01623724-c9b0-45d5-8e7b-c8a0a72ccfd1",               "Selected": false             }           ],           "ConditionExpression": "[e027817c-065c-4c0e-ae65-cdff1acfc040]=1",           "StataExportCaption": "78",           "Instructions": null,           "Enabled": true,           "Triggers": [             "e027817c-065c-4c0e-ae65-cdff1acfc040"           ]         }       ],       "Groups": []     },     {       "$type": "RavenQuestionnaire.Core.Entities.SubEntities.Complete.CompleteGroup, RavenQuestionnaire.Core",       "PublicKey": "26967ece-1440-4ae2-ad61-c1951cc9f438",       "Title": "1",       "Propagated": "None",       "Triggers": [],       "Questions": [],       "Groups": [         {           "$type": "RavenQuestionnaire.Core.Entities.SubEntities.Complete.CompleteGroup, RavenQuestionnaire.Core",           "PublicKey": "185ad0ac-4bb5-4443-bb95-8ce723ee003e",           "Title": "2",           "Propagated": "Propagated",           "Triggers": [],           "Questions": [             {               "$type": "RavenQuestionnaire.Core.Entities.SubEntities.Complete.CompleteQuestion, RavenQuestionnaire.Core",               "PublicKey": "ae56e864-ebfb-44d5-b732-4510c96fba55",               "Title": "ghj",               "QuestionType": "SingleOption",               "Answers": [                 {                   "$type": "RavenQuestionnaire.Core.Entities.SubEntities.Complete.CompleteAnswer, RavenQuestionnaire.Core",                   "PublicKey": "74be15fc-9e14-4f02-9127-bfd375e5b68b",                   "Title": "1",                   "Mandatory": false,                   "AnswerType": "Select",                   "AnswerValue": null,                   "QuestionPublicKey": "ae56e864-ebfb-44d5-b732-4510c96fba55",                   "Selected": false                 },                 {                   "$type": "RavenQuestionnaire.Core.Entities.SubEntities.Complete.CompleteAnswer, RavenQuestionnaire.Core",                   "PublicKey": "44a57c72-bfb1-4714-b936-bc903aa43e1f",                   "Title": "2",                   "Mandatory": false,                   "AnswerType": "Select",                   "AnswerValue": null,                   "QuestionPublicKey": "ae56e864-ebfb-44d5-b732-4510c96fba55",                   "Selected": false                 }               ],               "ConditionExpression": null,               "StataExportCaption": null,               "Instructions": null,               "Enabled": true,               "Triggers": []             },             {               "$type": "RavenQuestionnaire.Core.Entities.SubEntities.Complete.CompleteQuestion, RavenQuestionnaire.Core",               "PublicKey": "f8cbda1d-c4d9-43fa-9263-2adf35426640",               "Title": "ghj",               "QuestionType": "SingleOption",               "Answers": [                 {                   "$type": "RavenQuestionnaire.Core.Entities.SubEntities.Complete.CompleteAnswer, RavenQuestionnaire.Core",                   "PublicKey": "8fb55d1b-a102-4962-829e-fb4693a75f28",                   "Title": "76",                   "Mandatory": false,                   "AnswerType": "Select",                   "AnswerValue": "6",                   "QuestionPublicKey": "f8cbda1d-c4d9-43fa-9263-2adf35426640",                   "Selected": false                 },                 {                   "$type": "RavenQuestionnaire.Core.Entities.SubEntities.Complete.CompleteAnswer, RavenQuestionnaire.Core",                   "PublicKey": "572f0679-957f-4ec0-92b2-d69c3698c46f",                   "Title": "jhkhjk",                   "Mandatory": false,                   "AnswerType": "Select",                   "AnswerValue": "3",                   "QuestionPublicKey": "f8cbda1d-c4d9-43fa-9263-2adf35426640",                   "Selected": false                 }               ],               "ConditionExpression": "[ae56e864-ebfb-44d5-b732-4510c96fba55]>1",               "StataExportCaption": "65",               "Instructions": null,               "Enabled": true,               "Triggers": [                 "ae56e864-ebfb-44d5-b732-4510c96fba55"               ]             }           ],           "Groups": []         }       ]     }   ],   "Title": "test",   "CreationDate": "2012-03-21T18:30:00.1384078-04:00",   "LastEntryDate": "2012-03-21T18:30:00.1384078-04:00",   "OpenDate": null,   "CloseDate": null,   "PublicKey": "00000000-0000-0000-0000-000000000000",   "Propagated": "None",   "Triggers": [],   "ForcingPropagationPublicKey": null }'));
        alert(getFromStorage(qId).Questions.Length);
        /*  //when art detail pages load, show button
        $('div.artDetail').live('pageshow', function (event, ui) {
        //which do we show?
        var id = $(this).data("artid");
        if (!hasInStorage(id)) {
        $(".addToFavoritesDiv").show();
        $(".removeFromFavoritesDiv").hide();
        }
        else {
        $(".addToFavoritesDiv").hide();
        $(".removeFromFavoritesDiv").show();
        }
        });

        //When clicking the link in details pages to add to fav
        $(".addToFavoritesDiv a").live('vclick', function (event) {
        var id = $(this).data("artid");
        $.mobile.changePage("addtofav.cfm", { role: "dialog", data: { "id": id} });
        });

        //When clicking the link in details pages to add to fav
        $(".removeFromFavoritesDiv a").live('vclick', function (event) {
        var id = $(this).data("artid");
        $.mobile.changePage("removefromfav.cfm", { role: "dialog", data: { "id": id} });
        });

        //When confirming the add to fav
        $('.addToFavoritesButton').live('vclick', function (event, ui) {
        var id = $(this).data("artid");
        var label = $(this).data("artname");
        addToStorage(id, label);
        $("#addToFavoritesDialog").dialog("close");
        });

        //When confirming the remove from fav
        $('.removeFromFavoritesButton').live('vclick', function (event, ui) {
        var id = $(this).data("artid");
        var label = $(this).data("artname");
        removeFromStorage(id, label);
        $("#removeFromFavoritesDialog").dialog("close");
        });


        $('#homePage').live('pagebeforeshow', function (event, ui) {
        //get our favs
        var favs = getStorage();
        var $favoritesList = $("#favoritesList");
        if (!$.isEmptyObject(favs)) {
        if ($favoritesList.size() == 0) {
        $favoritesList = $('<ul id="favoritesList" data-inset="true"></ul>');

        var s = "<li data-role=\"list-divider\">Favorites</li>";
        for (var key in favs) {
        s += "<li><a href=\"art.cfm?id=" + key + "\">" + favs[key] + "</a></li>";
        }
        $favoritesList.append(s);
        $("#homePageContent").append($favoritesList);
        $favoritesList.listview();
        } else {
        $favoritesList.empty();
        var s = "<li data-role=\"list-divider\">Favorites</li>";
        for (var key in favs) {
        s += "<li><a href=\"art.cfm?id=" + key + "\">" + favs[key] + "</a></li>";
        }
        $favoritesList.append(s);
        $favoritesList.listview("refresh");
        }
        } else {
        // remove list if it exists and there are no favs
        if ($favoritesList.size() > 0) $favoritesList.remove();
        }
        });
        */

    }

});