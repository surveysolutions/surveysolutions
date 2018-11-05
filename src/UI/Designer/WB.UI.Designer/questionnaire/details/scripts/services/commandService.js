angular.module('designerApp')
    .factory('commandService',
        function ($http, blockUI, Upload, notificationService, $q) {

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
                }).then(function (response) {
                    blockUI.stop();
                    return response;
                }, function (response) {
                    blockUI.stop();
                    return $q.reject(response);
                });
            }

            commandService.execute = function (type, command) {
                return commandCall(type, command);
            };

            commandService.updateAttachment = function (questionnaireId, attachmentId, attachment) {
                blockUI.start();

                var command = {
                    questionnaireId: questionnaireId,
                    attachmentId: attachmentId,
                    attachmentName: attachment.name,
                    oldAttachmentId: attachment.oldAttachmentId
                };

                var fileName = "";
                if (!_.isUndefined(attachment.meta)) {
                    fileName = attachment.meta.fileName;
                }

                return Upload.upload({
                    url: urlBase + '/attachment',
                    data: { file: _.isNull(attachment.file) ? "" : attachment.file, fileName: fileName, "command": JSON.stringify(command) }
                }).then(function () {
                    blockUI.stop();
                }, function () {
                    blockUI.stop();
                });
            };

            commandService.deleteAttachment = function (questionnaireId, attachmentId) {
                var command = {
                    questionnaireId: questionnaireId,
                    attachmentId: attachmentId
                };
                return commandCall("DeleteAttachment", command);
            };

            commandService.updateLookupTable = function (questionnaireId, lookupTable) {
                blockUI.start();

                var command = {
                    questionnaireId: questionnaireId,
                    lookupTableId: lookupTable.itemId,
                    lookupTableName: lookupTable.name,
                    lookupTableFileName: lookupTable.fileName,
                    oldLookupTableId: lookupTable.oldItemId
                };

                return Upload.upload({
                    url: urlBase + '/UpdateLookupTable',
                    data: { file: _.isNull(lookupTable.file) ? "" : lookupTable.file, "command": JSON.stringify(command) }
                }).then(function () {
                    blockUI.stop();
                }).catch(function (err) {
                    blockUI.stop();
                    throw err; 
                });
            };

            commandService.addLookupTable = function (questionnaireId, lookupTable) {
                var command = {
                    questionnaireId: questionnaireId,
                    lookupTableId: lookupTable.itemId
                };
                return commandCall("AddLookupTable", command);
            };

            commandService.deleteLookupTable = function (questionnaireId, itemId) {
                var command = {
                    questionnaireId: questionnaireId,
                    lookupTableId: itemId
                };
                return commandCall("DeleteLookupTable", command);
            };

            commandService.updateTranslation = function (questionnaireId, translation) {
                blockUI.start();

                var command = {
                    questionnaireId: questionnaireId,
                    translationId: translation.translationId,
                    oldTranslationId: translation.oldTranslationId,
                    name: translation.name
                };

                return Upload.upload({
                    url: urlBase + '/translation',
                    data: { file: _.isNull(translation.file) ? "" : translation.file, "command": JSON.stringify(command) }
                }).then(function (response) {
                    blockUI.stop();

                    if (!_.isNull(translation.file))
                        notificationService.notice(response.data);

                    translation.file = null;

                }).catch(function () {
                    blockUI.stop();
                });
            };

            commandService.deleteTranslation = function (questionnaireId, translationId) {
                var command = {
                    questionnaireId: questionnaireId,
                    translationId: translationId
                };
                return commandCall("DeleteTranslation", command);
            };

            commandService.setDefaultTranslation = function(questionnaireId, translationId) {
                var command = {
                    questionnaireId: questionnaireId,
                    translationId: translationId
                };
                return commandCall("SetDefaultTranslation", command);
            };

            commandService.addMacro = function (questionnaireId, macro) {
                var command = {
                    questionnaireId: questionnaireId,
                    macroId: macro.itemId
                };
                return commandCall("AddMacro", command);
            };

            commandService.updateMacro = function (questionnaireId, macro) {
                var command = {
                    questionnaireId: questionnaireId,
                    macroId: macro.itemId,
                    name: macro.name,
                    content: macro.content,
                    description: macro.description
                };

                return commandCall("UpdateMacro", command);
            };

            commandService.deleteMacros = function (questionnaireId, itemId) {
                var command = {
                    questionnaireId: questionnaireId,
                    macroId: itemId
                };
                return commandCall("DeleteMacro", command);
            };

            commandService.updateMetadata = function (questionnaireId, metadata) {
                var command = {
                    questionnaireId: questionnaireId,
                    title: metadata.title,
                    metadata: {
                        subTitle: metadata.subTitle,
                        studyType: metadata.studyType,
                        version: metadata.version,
                        versionNotes: metadata.versionNotes,
                        kindOfData: metadata.kindOfData,
                        country: metadata.country,
                        year: metadata.year,
                        language: metadata.language,
                        coverage: metadata.coverage,
                        universe: metadata.universe,
                        unitOfAnalysis: metadata.unitOfAnalysis,
                        primaryInvestigator: metadata.primaryInvestigator,
                        funding: metadata.funding,
                        consultant: metadata.consultant,
                        modeOfDataCollection: metadata.modeOfDataCollection,
                        notes: metadata.notes,
                        keywords: metadata.keywords,
                        agreeToMakeThisQuestionnairePublic: metadata.agreeToMakeThisQuestionnairePublic
                    }
                };

                return commandCall("UpdateMetadata", command);
            };


            commandService.sendUpdateQuestionCommand = function (questionnaireId, question, shouldGetOptionsOnServer) {

                var command = {
                    questionnaireId: questionnaireId,
                    questionId: question.itemId,
                    type: question.type,
                    mask: question.mask,
                    validationConditions: question.validationConditions,

                    commonQuestionParameters: {
                        title: question.title,
                        variableName: question.variable,
                        variableLabel: question.variableLabel,
                        enablementCondition: question.enablementCondition,
                        hideIfDisabled: question.hideIfDisabled,
                        instructions: question.instructions,
                        hideInstructions: question.hideInstructions,
                        optionsFilterExpression: question.optionsFilterExpression,
                        geometryType: question.geometryType
                    }
                };

                var isPrefilledScopeSelected = question.questionScope === 'Identifying';
                command.isPreFilled = isPrefilledScopeSelected;
                command.scope = isPrefilledScopeSelected ? 'Interviewer' : question.questionScope;

                switch (question.type) {
                    case "SingleOption":
                        command.areAnswersOrdered = question.areAnswersOrdered;
                        command.maxAllowedAnswers = question.maxAllowedAnswers;
                        command.linkedToEntityId = question.linkedToEntityId;
                        command.linkedFilterExpression = question.linkedFilterExpression;
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
                        command.linkedFilterExpression = question.linkedFilterExpression;
                        command.yesNoView = question.yesNoView;
                        command.options = _.isEmpty(command.linkedToEntityId) ? question.options : null;
                        break;
                    case "Numeric":
                        command.isInteger = question.isInteger;
                        command.countOfDecimalPlaces = question.countOfDecimalPlaces;
                        command.maxValue = question.maxValue;
                        command.useFormatting = question.useFormatting;
                        command.options = question.options;
                        break;
                    case "DateTime":
                        command.isTimestamp = question.isTimestamp;
                        command.defaultDate = question.defaultDate;
                        break;
                    case "GpsCoordinates":
                    case "Text":
                    case "Area":
                        break;
                    case "Audio":
                        command.quality = question.quality;
                        break;
                    case "TextList":
                        command.maxAnswerCount = question.maxAnswerCount;
                        break;
                    case "Multimedia":
                        command.IsSignature = question.isSignature;
                        break;
                }
                var questionType = question.type === "MultyOption" ? "MultiOption" : question.type; // we have different name in enum and in command. Correct one is 'Multi' but we cant change it in enum
                var commandName = "Update" + questionType + "Question";

                return commandCall(commandName, command);
            };

            commandService.addChapter = function (questionnaireId, chapter) {
                var command = {
                    questionnaireId: questionnaireId,
                    groupId: chapter.itemId,
                    title: chapter.title,
                    condition: "",
                    hideIfDisabled: false,
                    isRoster: false,
                    rosterSizeQuestionId: null,
                    rosterSizeSource: "Question",
                    rosterFixedTitles: null,
                    rosterTitleQuestionId: null,
                    parentGroupId: null,
                    variableName: null
                };

                return commandCall("AddGroup", command);
            };

            commandService.addGroup = function (questionnaireId, group, parentGroupId, index) {
                var command = {
                    questionnaireId: questionnaireId,
                    groupId: group.itemId,
                    title: group.title,
                    condition: "",
                    hideIfDisabled: false,
                    isRoster: false,
                    rosterSizeQuestionId: null,
                    rosterSizeSource: "Question",
                    rosterFixedTitles: null,
                    rosterTitleQuestionId: null,
                    parentGroupId: parentGroupId,
                    variableName: null,
                    index: index
                };
                return commandCall("AddGroup", command);
            };

            commandService.addRoster = function (questionnaireId, group, parentGroupId, index) {
                var command = {
                    questionnaireId: questionnaireId,
                    groupId: group.itemId,
                    title: group.title,
                    condition: "",
                    hideIfDisabled: false,
                    isRoster: true,
                    rosterSizeQuestionId: null,
                    rosterSizeSource: "FixedTitles",
                    fixedRosterTitles: [{ value: 1, title: "First Title" }, { value: 2, title: "Second Title" }],
                    rosterTitleQuestionId: null,
                    parentGroupId: parentGroupId,
                    variableName: group.variableName,
                    index: index
                };

                return commandCall("AddGroup", command);
            };

            commandService.addStaticText = function (questionnaireId, staticText, parentId, index) {
                var command = {
                    questionnaireId: questionnaireId,
                    entityId: staticText.itemId,
                    text: staticText.text,
                    parentId: parentId,
                    index: index
                };
                return commandCall("AddStaticText", command);
            };

            commandService.addVariable = function (questionnaireId, variable, parentId, index) {
                var command = {
                    questionnaireId: questionnaireId,
                    entityId: variable.itemId,
                    parentId: parentId,
                    index: index,
                    variableData: {}
                };
                return commandCall("AddVariable", command);
            };

            commandService.updateVariable = function (questionnaireId, variable) {
                var command = {
                    questionnaireId: questionnaireId,
                    entityId: variable.itemId,
                    variableData: {
                        expression: variable.expression,
                        name: variable.variable,
                        type: variable.type,
                        label: variable.label
                    }
                }

                return commandCall("UpdateVariable", command);
            }

            commandService.updateGroup = function (questionnaireId, group) {
                var command = {
                    questionnaireId: questionnaireId,
                    groupId: group.itemId,
                    title: group.title,
                    condition: group.enablementCondition,
                    hideIfDisabled: group.hideIfDisabled,
                    isRoster: false,
                    rosterSizeQuestionId: null,
                    rosterSizeSource: "Question",
                    rosterFixedTitles: null,
                    rosterTitleQuestionId: null,
                    variableName: null
                };

                return commandCall("UpdateGroup", command);
            };

            commandService.updateRoster = function (questionnaireId, incomingRoster) {

                var command = {
                    questionnaireId: questionnaireId,
                    groupId: incomingRoster.itemId,
                    title: incomingRoster.title,
                    description: incomingRoster.description,
                    condition: incomingRoster.enablementCondition,
                    hideIfDisabled: incomingRoster.hideIfDisabled,
                    variableName: incomingRoster.variableName,
                    isRoster: true
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
                    questionnaireId: questionnaireId,
                    entityId: staticText.itemId,
                    text: staticText.text,
                    attachmentName: staticText.attachmentName,
                    enablementCondition: staticText.enablementCondition,
                    hideIfDisabled: staticText.hideIfDisabled,
                    validationConditions: staticText.validationConditions
                };

                return commandCall("UpdateStaticText", command);
            };

            commandService.addQuestion = function (questionnaireId, parentGroupId, newId, index) {
                var command = {
                    questionnaireId: questionnaireId,
                    parentGroupId: parentGroupId,
                    questionId: newId,
                    index: index
                };

                return commandCall("AddDefaultTypeQuestion", command);
            };

            commandService.deleteGroup = function (questionnaireId, itemId) {
                var command = {
                    questionnaireId: questionnaireId,
                    groupId: itemId
                };

                return commandCall("DeleteGroup", command);
            };

            commandService.deleteQuestion = function (questionnaireId, itemId) {
                var command = {
                    questionnaireId: questionnaireId,
                    questionId: itemId
                };

                return commandCall("DeleteQuestion", command);
            };

            commandService.deleteVariable = function (questionnaireId, itemId) {
                var command = {
                    questionnaireId: questionnaireId,
                    entityId: itemId
                };

                return commandCall("DeleteVariable", command);
            };

            commandService.deleteStaticText = function (questionnaireId, itemId) {
                var command = {
                    questionnaireId: questionnaireId,
                    entityId: itemId
                };

                return commandCall("DeleteStaticText", command);
            };

            commandService.pasteItemAfter = function (questionnaireId, itemToPasteAfterId, sourceQuestionnaireId, sourceItemId, newId) {
                return commandCall('PasteAfter', {
                    sourceQuestionnaireId: sourceQuestionnaireId,
                    sourceItemId: sourceItemId,
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
    );
