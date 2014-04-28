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
                    "groupId": chapter.ChapterId,
                    "title": chapter.Title,
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
                    "groupId": group.Id,
                    "title": group.Title,
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
                    "parentGroupId": group.Id
                };

                return commandCall("AddQuestion", command);
            };

            commandService.cloneGroupWithoutChildren = function(questionnaireId, newId, chapter, chapterDescription) {
                var command = {
                    "questionnaireId": questionnaireId,
                    "groupId": newId,
                    "title": chapter.Title,
                    "description": chapterDescription,
                    "condition": "",
                    "isRoster": false,
                    "rosterSizeQuestionId": null,
                    "rosterSizeSource": "Question",
                    "rosterFixedTitles": null,
                    "rosterTitleQuestionId": null,
                    "parentGroupId": null,
                    "sourceGroupId": chapter.ChapterId,
                    "targetIndex": 1
                };

                return commandCall("CloneGroupWithoutChildren", command);
            };

            commandService.deleteGroup = function(questionnaireId, chapter) {
                var command = {
                    "questionnaireId": questionnaireId,
                    "groupId": chapter.ChapterId
                };

                return commandCall("DeleteGroup", command);
            };

            return commandService;
        }
    ]);