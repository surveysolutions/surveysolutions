angular.module('designerApp')
    .factory('commandService', [
        '$http', function($http) {

            var urlBase = 'command/execute';
            var commandService = {};

            function commandCall(type, command) {
                return $http({
                    method: 'POST',
                    url: urlBase,
                    data: {
                        "type": type,
                        "command": JSON.stringify(command)
                    },
                    headers: { 'Content-Type': 'application/json; ' }
                });
            }

            commandService.addChapter = function(questionnaireId, chapter) {
                var command = {
                    "questionnaireId": questionnaireId,
                    "groupId": chapter.chapterId,
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
                    "groupId": group.id,
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

            commandService.updateGroup = function (questionnaireId, group) {
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

            commandService.addQuestion = function(questionnaireId, group, newId) {
                var command = {
                    "questionnaireId": questionnaireId,
                    "questionId": newId,
                    "title": "New Question",
                    "type": "Text",
                    "variableName": "",
                    "isPreFilled": false,
                    "isMandatory": false,
                    "scope": "Interviewer",
                    "enablementCondition": "",
                    "validationExpression": "",
                    "validationMessage": "",
                    "instructions": "",
                    "parentGroupId": group.id
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

            commandService.deleteGroup = function(questionnaireId, chapter) {
                var command = {
                    "questionnaireId": questionnaireId,
                    "groupId": chapter.chapterId
                };

                return commandCall("DeleteGroup", command);
            };

            return commandService;
        }
    ]);