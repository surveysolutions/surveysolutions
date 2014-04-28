angular.module('designerApp')
    .factory('commandService', [
        '$http', function($http) {

            var urlBase = 'command/execute';
            var commandService = {};

            commandService.addChapter = function(questionnaireId, chapter) {
                return $http({
                    method: 'POST',
                    url: urlBase,
                    data: {
                        "type": "AddGroup",
                        "command": JSON.stringify({
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
                        })
                    },
                    headers: { 'Content-Type': 'application/json; ' }
                });
            };

            commandService.addGroup = function(questionnaireId, group, parentGroupId) {
                return $http({
                    method: 'POST',
                    url: urlBase,
                    data: {
                        "type": "AddGroup",
                        "command": JSON.stringify({
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
                        })
                    },
                    headers: { 'Content-Type': 'application/json; ' }
                });
            };

            commandService.addQuestion = function(questionnaireId, group, newId) {
                return $http({
                    method: 'POST',
                    url: urlBase,
                    data: {
                        "type": "AddQuestion",
                        "command": JSON.stringify({
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
                        })
                    },
                    headers: { 'Content-Type': 'application/json; ' }
                });
            };

            commandService.cloneGroupWithoutChildren = function(questionnaireId, newId, chapter, chapterDescription) {
                return $http({
                    method: 'POST',
                    url: urlBase,
                    data: {
                        "type": "CloneGroupWithoutChildren",
                        "command": JSON.stringify({
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
                        })
                    },
                    headers: { 'Content-Type': 'application/json; ' }
                });
            };

            commandService.deleteGroup = function(questionnaireId, chapter) {
                return $http({
                    method: 'POST',
                    url: urlBase,
                    data: {
                        "type": "DeleteGroup",
                        "command": JSON.stringify(
                            {
                                "questionnaireId": questionnaireId,
                                "groupId": chapter.ChapterId
                            }
                        )
                    },
                    headers: { 'Content-Type': 'application/json;' }
                });
            };

            return commandService;
        }
    ]);