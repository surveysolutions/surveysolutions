(function() {
    angular.module('designerApp')
        .factory('commandService', [
            '$http', 'blockUI',
            function ($http, blockUI) {

                var urlBase = '../../api/command';
                var commandService = {};

                function commandCall(type, command) {
                    blockUI.start();

                    return $http({
                        method: 'POST',
                        url: urlBase,
                        data: {
                            "type": type,
                            "command": JSON.stringify(command)
                        },
                        headers: { 'Content-Type': 'application/json', 'Accept': 'application/json' }
                    }).success(function() {
                         blockUI.stop();
                    }).error(function() {
                        blockUI.stop();
                    });
                }

                commandService.execute = function(type, command) {
                    return commandCall(type, command);
                };

                commandService.cloneQuestion = function(questionnaireId, itemIdToClone, newId) {
                    return commandCall('CloneQuestionById', {
                        questionId: itemIdToClone,
                        targetId: newId,
                        questionnaireId: questionnaireId
                    });
                };

                commandService.cloneStaticText = function (questionnaireId, itemIdToClone, newId) {
                    return commandCall('CloneStaticText', {
                        sourceEntityId: itemIdToClone,
                        entityId: newId,
                        questionnaireId: questionnaireId
                    });
                };

                commandService.cloneGroup = function (questionnaireId, groupIdToClone, targetIndex, newId) {

                    return commandCall('CloneGroup', {
                        sourceGroupId: groupIdToClone,
                        targetIndex: targetIndex,
                        groupId: newId,
                        questionnaireId: questionnaireId
                    });
                };

                commandService.sendUpdateQuestionCommand = function(questionnaireId, question) {
                    var isPrefilledScopeSelected = question.questionScope == 'Prefilled';

                    var command = {
                        questionnaireId: questionnaireId,
                        questionId: question.itemId,
                        title: question.title,
                        type: question.type,
                        variableName: question.variable,
                        variableLabel: question.variableLabel,
                        mask: question.mask,
                        isPreFilled: isPrefilledScopeSelected,
                        isMandatory: question.isMandatory,
                        scope: isPrefilledScopeSelected ? 'Interviewer' : question.questionScope,
                        enablementCondition: question.enablementCondition,
                        validationExpression: question.validationExpression,
                        validationMessage: question.validationMessage,
                        instructions: question.instructions
                    };

                    switch (question.type) {
                    case "SingleOption":
                    case "MultyOption":
                        command.areAnswersOrdered = question.areAnswersOrdered;
                        command.maxAllowedAnswers = question.maxAllowedAnswers;
                        command.options = question.options;
                        command.linkedToQuestionId = question.linkedToQuestionId;
                        break;
                    case "Numeric":
                        command.isInteger = question.isInteger;
                        command.countOfDecimalPlaces = question.countOfDecimalPlaces;
                        command.maxValue = question.maxValue;
                        break;
                    case "DateTime":
                    case "GpsCoordinates":
                    case "Text":
                        break;
                    case "TextList":
                        command.maxAnswerCount = 10;
                        break;
                    }
                    var questionType = question.type == "MultyOption" ? "MultiOption" : question.type; // we have different name in enum and in command. Correct one is 'Multi' but we cant change it in enum
                    var commandName = "Update" + questionType + "Question";

                    return commandCall(commandName, command);
                };

                commandService.addChapter = function(questionnaireId, chapter) {
                    var command = {
                        "questionnaireId": questionnaireId,
                        "groupId": chapter.itemId,
                        "title": chapter.title,
                        "description": "",
                        "condition": "",
                        "isRoster": false,
                        "rosterSizeQuestionId": null,
                        "rosterSizeSource": "Question",
                        "rosterFixedTitles": null,
                        "rosterTitleQuestionId": null,
                        "parentGroupId": null,
                        "variableName": null
                    };

                    return commandCall("AddGroup", command);
                };

                commandService.addGroup = function(questionnaireId, group, parentGroupId) {
                    var command = {
                        "questionnaireId": questionnaireId,
                        "groupId": group.itemId,
                        "title": group.title,
                        "description": "",
                        "condition": "",
                        "isRoster": false,
                        "rosterSizeQuestionId": null,
                        "rosterSizeSource": "Question",
                        "rosterFixedTitles": null,
                        "rosterTitleQuestionId": null,
                        "parentGroupId": parentGroupId,
                        "variableName":null
                    };
                    return commandCall("AddGroup", command);
                };

                commandService.addRoster = function(questionnaireId, group, parentGroupId) {
                    var command = {
                        "questionnaireId": questionnaireId,
                        "groupId": group.itemId,
                        "title": group.title,
                        "description": "",
                        "condition": "",
                        "isRoster": true,
                        "rosterSizeQuestionId": null,
                        "rosterSizeSource": "FixedTitles",
                        "rosterFixedTitles": ["Title"],
                        "rosterTitleQuestionId": null,
                        "parentGroupId": parentGroupId,
                        "variableName": group.variableName
                    };

                    return commandCall("AddGroup", command);
                };

                commandService.addStaticText = function (questionnaireId, staticText, parentId) {
                    var command = {
                        "questionnaireId": questionnaireId,
                        "entityId": staticText.itemId,
                        "text": staticText.text,
                        "parentId": parentId
                    };
                    return commandCall("AddStaticText", command);
                };

                commandService.updateGroup = function(questionnaireId, group) {
                    var command = {
                        "questionnaireId": questionnaireId,
                        "groupId": group.itemId,
                        "title": group.title,
                        "description": group.description,
                        "condition": group.enablementCondition,
                        "isRoster": false,
                        "rosterSizeQuestionId": null,
                        "rosterSizeSource": "Question",
                        "rosterFixedTitles": null,
                        "rosterTitleQuestionId": null,
                        "variableName":null
                    };

                    return commandCall("UpdateGroup", command);
                };

                commandService.updateRoster = function(questionnaireId, incomingRoster) {

                    var command = {
                        "questionnaireId": questionnaireId,
                        "groupId": incomingRoster.itemId,
                        "title": incomingRoster.title,
                        "description": incomingRoster.description,
                        "condition": incomingRoster.enablementCondition,
                        "variableName": incomingRoster.variableName,
                        "isRoster": true
                    };

                    switch (incomingRoster.type) {
                    case "Fixed":
                        command.rosterSizeSource = "FixedTitles";
                        command.rosterFixedTitles = incomingRoster.rosterFixedTitles;
                        break;
                    case "Numeric":
                        command.rosterSizeQuestionId = incomingRoster.rosterSizeNumericQuestionId;
                        command.rosterTitleQuestionId = incomingRoster.rosterTitleQuestionId;
                        break;
                    case "List":
                        command.rosterSizeQuestionId = incomingRoster.rosterSizeListQuestionId;
                        break;
                    case "Multi":
                        command.rosterSizeQuestionId = incomingRoster.rosterSizeMultiQuestionId;
                        break;
                    }

                    return commandCall("UpdateGroup", command);
                };
                
                commandService.updateStaticText = function (questionnaireId, staticText) {
                    var command = {
                        "questionnaireId": questionnaireId,
                        "entityId": staticText.itemId,
                        "text": staticText.text
                    };

                    return commandCall("UpdateStaticText", command);
                };

                commandService.addQuestion = function(questionnaireId, parentGroupId, newId) {                    
                    var command = {
                        "questionnaireId": questionnaireId,
                        "questionId": newId,
                        "title": "New Question",
                        "type": "Text",
                        "isPreFilled": false,
                        "isMandatory": false,
                        "scope": "Interviewer",
                        "enablementCondition": "",
                        "validationExpression": "",
                        "validationMessage": "",
                        "instructions": "",
                        "parentGroupId": parentGroupId
                    };

                    return commandCall("AddQuestion", command);
                };


                commandService.cloneGroupWithoutChildren = function(questionnaireId, newId, chapter, chapterDescription) {
                    var command = {
                        "questionnaireId": questionnaireId,
                        "groupId": newId,
                        "title": chapter.title,
                        "description": chapterDescription,
                        "condition": "",
                        "isRoster": false,
                        "rosterSizeQuestionId": null,
                        "rosterSizeSource": "Question",
                        "rosterFixedTitles": null,
                        "rosterTitleQuestionId": null,
                        "parentGroupId": null,
                        "sourceGroupId": chapter.chapterId,
                        "targetIndex": 1
                    };

                    return commandCall("CloneGroupWithoutChildren", command);
                };

                commandService.deleteGroup = function(questionnaireId, itemId) {
                    var command = {
                        "questionnaireId": questionnaireId,
                        "groupId": itemId
                    };

                    return commandCall("DeleteGroup", command);
                };

                commandService.deleteQuestion = function (questionnaireId, itemId) {
                    var command = {
                        "questionnaireId": questionnaireId,
                        "questionId": itemId
                    };

                    return commandCall("DeleteQuestion", command);
                };

                commandService.deleteStaticText = function (questionnaireId, itemId) {
                    var command = {
                        "questionnaireId": questionnaireId,
                        "entityId": itemId
                    };

                    return commandCall("DeleteStaticText", command);
                };

                return commandService;
            }
        ]);
}());