(function() {
    angular.module('designerApp')
        .factory('commandService', [
            '$http', 'blockUI', 'Upload',
            function ($http, blockUI, Upload) {

                var urlBase = '../../api/command';
                var commandService = {};

                function commandCall(type, command) {
                    if (type.indexOf('Move') < 0) {
                        blockUI.start();
                    }
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

                commandService.updateLookupTable = function (questionnaireId, lookupTable) {
                    blockUI.start();

                    var command = {
                        "questionnaireId": questionnaireId,
                        "lookupTableId": lookupTable.itemId,
                        "lookupTableName": lookupTable.name,
                        "lookupTableFileName": lookupTable.fileName
                    };

                    return Upload.upload({
                        url: urlBase + '/UpdateLookupTable',
                        data: { file: _.isNull(lookupTable.file) ? "" : lookupTable.file, "command": JSON.stringify(command) }
                    }).success(function () {
                        blockUI.stop();
                    }).error(function () {
                        blockUI.stop();
                    });
                };

                commandService.addLookupTable = function (questionnaireId, lookupTable) {
                    var command = {
                        "questionnaireId": questionnaireId,
                        "lookupTableId": lookupTable.itemId
                    };
                    return commandCall("AddLookupTable", command);
                };

                commandService.deleteLookupTable = function (questionnaireId, itemId) {
                    var command = {
                        "questionnaireId": questionnaireId,
                        "lookupTableId": itemId
                    };
                    return commandCall("DeleteLookupTable", command);
                };

                commandService.addMacro = function (questionnaireId, macro) {
                    var command = {
                        "questionnaireId": questionnaireId,
                        "macroId": macro.itemId
                    };
                    return commandCall("AddMacro", command);
                };

                commandService.updateMacro = function (questionnaireId, macro) {
                    var command = {
                        "questionnaireId": questionnaireId,
                        "macroId": macro.itemId,
                        "name": macro.name,
                        "content": macro.content,
                        "description": macro.description
                    };

                    return commandCall("UpdateMacro", command);
                };

                commandService.deleteMacros = function(questionnaireId, itemId) {
                    var command = {
                        "questionnaireId": questionnaireId,
                        "macroId": itemId
                    };
                    return commandCall("DeleteMacro", command);
                };

                commandService.sendUpdateQuestionCommand = function(questionnaireId, question, shouldGetOptionsOnServer) {

                    var command = {
                        questionnaireId: questionnaireId,
                        questionId: question.itemId,
                        title: question.title,
                        type: question.type,
                        variableName: question.variable,
                        variableLabel: question.variableLabel,
                        mask: question.mask,
                        enablementCondition: question.enablementCondition,
                        validationExpression: question.validationExpression,
                        validationMessage: question.validationMessage,
                        instructions: question.instructions
                    };

                    var isPrefilledScopeSelected = question.questionScope == 'Prefilled';
                    command.isPreFilled = isPrefilledScopeSelected;
                    command.scope = isPrefilledScopeSelected ? 'Interviewer' : question.questionScope;

                    switch (question.type) {
                    case "SingleOption":
                        command.areAnswersOrdered = question.areAnswersOrdered;
                        command.maxAllowedAnswers = question.maxAllowedAnswers;
                        command.linkedToEntityId = question.linkedToEntityId;
                        command.isFilteredCombobox = question.isFilteredCombobox || false;
                        command.cascadeFromQuestionId = question.cascadeFromQuestionId;
                        command.enablementCondition = question.cascadeFromQuestionId ? '' : command.enablementCondition;
                        command.validationExpression = question.cascadeFromQuestionId ? '' : command.validationExpression;
                        command.validationMessage = question.cascadeFromQuestionId ? '' : command.validationMessage;
                        if (shouldGetOptionsOnServer) {
                            command.options = null;
                        } else {
                            command.options = question.options;
                        }
                        break;
                    case "MultyOption":
                        command.areAnswersOrdered = question.areAnswersOrdered;
                        command.maxAllowedAnswers = question.maxAllowedAnswers;
                        command.linkedToEntityId = question.linkedToEntityId;
                        command.yesNoView = question.yesNoView;
                        command.options = _.isEmpty(command.linkedToEntityId) ? question.options : null;
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
                        command.maxAnswerCount = question.maxAnswerCount;
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

                commandService.addGroup = function (questionnaireId, group, parentGroupId, index) {
                    var command = {
                        "questionnaireId": questionnaireId,
                        "groupId": group.itemId,
                        "title": group.title,
                        "condition": "",
                        "isRoster": false,
                        "rosterSizeQuestionId": null,
                        "rosterSizeSource": "Question",
                        "rosterFixedTitles": null,
                        "rosterTitleQuestionId": null,
                        "parentGroupId": parentGroupId,
                        "variableName": null,
                        "index": index
                    };
                    return commandCall("AddGroup", command);
                };

                commandService.addRoster = function (questionnaireId, group, parentGroupId, index) {
                    var command = {
                        "questionnaireId": questionnaireId,
                        "groupId": group.itemId,
                        "title": group.title,
                        "condition": "",
                        "isRoster": true,
                        "rosterSizeQuestionId": null,
                        "rosterSizeSource": "FixedTitles",
                        "fixedRosterTitles": [{ value: 1, title: "First Title" }, { value:2, title: "Second Title" }],
                        "rosterTitleQuestionId": null,
                        "parentGroupId": parentGroupId,
                        "variableName": group.variableName,
                        "index": index
                    };

                    return commandCall("AddGroup", command);
                };

                commandService.addStaticText = function (questionnaireId, staticText, parentId, index) {
                    var command = {
                        "questionnaireId": questionnaireId,
                        "entityId": staticText.itemId,
                        "text": staticText.text,
                        "parentId": parentId,
                        "index": index
                    };
                    return commandCall("AddStaticText", command);
                };

                commandService.updateGroup = function(questionnaireId, group) {
                    var command = {
                        "questionnaireId": questionnaireId,
                        "groupId": group.itemId,
                        "title": group.title,
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
                        command.fixedRosterTitles = incomingRoster.fixedRosterTitles;
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

                commandService.addQuestion = function (questionnaireId, parentGroupId, newId, index) {
                    var command = {
                        "questionnaireId": questionnaireId,
                        "parentGroupId": parentGroupId,
                        "questionId": newId,
                        "index": index
                    };

                    return commandCall("AddDefaultTypeQuestion", command);
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

                commandService.pasteItemAfter = function (questionnaireId, itemToPasteAfterId, sourceQuestionnaireId, sourceItemId, newId) {
                    return commandCall('PasteAfter', {
                        sourceQuestionnaireId: sourceQuestionnaireId,
                        sourceItemId : sourceItemId,
                        itemToPasteAfterId: itemToPasteAfterId,
                        entityId: newId,
                        questionnaireId: questionnaireId
                    });
                };

                commandService.pasteItemInto = function (questionnaireId, parentGroupId, sourceQuestionnaireId, sourceItemId, newId) {
                    return commandCall('PasteInto', {
                        sourceQuestionnaireId: sourceQuestionnaireId,
                        sourceItemId: sourceItemId,
                        parentId: parentGroupId,
                        entityId: newId,
                        questionnaireId: questionnaireId
                    });
                };

                return commandService;
            }
        ]);
}());