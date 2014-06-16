(function() {
    'use strict';
    angular.module('designerApp')
        .factory('questionnaireService', [
            '$http', 'commandService', 'utilityService',
            function($http, commandService, string) {

                var urlBase = '../api/questionnaire';
                var questionnaireService = {};

                questionnaireService.getQuestionnaireById = function(questionnaireId) {
                    var url = string.format('{0}/get/{1}', urlBase, questionnaireId);
                    return $http.get(url);
                };

                questionnaireService.getChapterById = function(questionnaireId, chapterId) {
                    var url = string.format('{0}/chapter/{1}?chapterId={2}', urlBase, questionnaireId, chapterId);
                    return $http.get(url);
                };

                questionnaireService.getGroupDetailsById = function(questionnaireId, groupId) {
                    var url = string.format('{0}/editGroup/{1}?groupId={2}', urlBase, questionnaireId, groupId);
                    return $http.get(url);
                };

                questionnaireService.getRosterDetailsById = function(questionnaireId, rosterId) {
                    var url = string.format('{0}/editRoster/{1}?rosterId={2}', urlBase, questionnaireId, rosterId);
                    return $http.get(url);
                };

                questionnaireService.getQuestionDetailsById = function(questionnaireId, questionId) {
                    var url = string.format('{0}/editQuestion/{1}?questionId={2}', urlBase, questionnaireId, questionId);
                    return $http.get(url);
                };

                questionnaireService.moveGroup = function(groupId, index, destGroupId, questionnaireId) {
                    commandService.execute('MoveGroup', {
                            targetGroupId: destGroupId,
                            targetIndex: index,
                            groupId: groupId,
                            questionnaireId: questionnaireId
                        }
                    );
                };

                questionnaireService.moveQuestion = function(questionId, index, destGroupId, questionnaireId) {
                    commandService.execute('MoveQuestion', {
                            targetGroupId: destGroupId,
                            targetIndex: index,
                            questionId: questionId,
                            questionnaireId: questionnaireId
                        }
                    );
                };

                

                questionnaireService.removeItem = function (items, itemId) {
                    var removeFunc = function(item, itemToRemoveId) {
                        var itemToRemove = _.findWhere(items, { itemId: itemId });
                        if (itemToRemove) {
                            var indexOf = _.indexOf(items, itemToRemove);
                            items.splice(indexOf, 1);
                            return true;
                        }

                        var childItems = item.items;
                        for (var i = 0; i < childItems.length; i++) {
                            removeFunc(childItems[i], itemToRemoveId);
                        }

                        return false;
                    };

                    for (var j = 0; j < items.length; j++) {

                        if (removeFunc(items[j], itemId)) {
                            return;
                        };
                    }
                   
                }

                return questionnaireService;
            }
        ]);

}());