(function() {
    angular.module('designerApp')
        .factory('commandService', [
            '$http', function($http) {

                var urlBase = '../command/execute';
                var commandService = {};

                function commandCall(type, command) {
                    return $http({
                        method: 'POST',
                        url: urlBase,
                        data: {
                            "type": type,
                            "command": JSON.stringify(command)
                        },
                        headers: {'Content-Type': 'application/json;'}
                    });
                }

                commandService.execute = function(type, command) {
                    return commandCall(type, command);
                };

                commandService.sendUpdateQuestionCommand = function(questionnaireId, question) {
                    var command = {
                        questionnaireId: questionnaireId,
                        questionId: question.itemId,
                        title: question.title,
                        type: question.type,
                        variableName: question.variable,
                        variableLabel: question.variableLabel,
                        isPreFilled: question.questionScope == 'Headquarter',
                        isMandatory: question.isMandatory,
                        scope: question.questionScope,
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

                    var commandName = "Update" + question.type + "Question";

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
                        "parentGroupId": null
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
                        "parentGroupId": parentGroupId
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
                        "rosterFixedTitles": ["111"], // todo: temp solution
                        "rosterTitleQuestionId": null,
                        "parentGroupId": parentGroupId
                    };

                    return commandCall("AddGroup", command);
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
                        "rosterTitleQuestionId": null
                    };

                    return commandCall("UpdateGroup", command);
                };

                commandService.updateRoster = function(questionnaireId, roster) {
                    var command = {
                        "questionnaireId": questionnaireId,
                        "groupId": roster.itemId,
                        "title": roster.title,
                        "description": roster.description,
                        "condition": roster.enablementCondition,
                        "isRoster": true,
                        "rosterSizeQuestionId": roster.rosterSizeQuestionId,
                        "rosterSizeSource": roster.rosterSizeSourceType,
                        "rosterFixedTitles": roster.rosterFixedTitles,
                        "rosterTitleQuestionId": roster.rosterTitleQuestionId
                    };

                    return commandCall("UpdateGroup", command);
                };

                commandService.addQuestion = function(questionnaireId, parentGroupId, newId, varName) {
                    var command = {
                        "questionnaireId": questionnaireId,
                        "questionId": newId,
                        "title": "New Question",
                        "type": "Text",
                        "variableName": varName,
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

                return commandService;
            }
        ]);
}());