(function() {
    'use strict';
    angular.module('designerApp')
        .factory('questionnaireService', [
            '$http', 'commandService', 'utilityService',
            function($http, commandService, string) {

                var urlBase = '../../api/questionnaire';
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

                questionnaireService.getAllBrokenGroupDependencies = function (questionnaireId, groupId) {
                    var url = string.format('{0}/getAllBrokenGroupDependencies/{1}?groupId={2}', urlBase, questionnaireId, groupId);
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

                questionnaireService.getStaticTextDetailsById = function (questionnaireId, staticTextId) {
                    var url = string.format('{0}/editStaticText/{1}?staticTextId={2}', urlBase, questionnaireId, staticTextId);
                    return $http.get(url);
                };

                questionnaireService.moveGroup = function(groupId, index, destGroupId, questionnaireId) {
                    return commandService.execute('MoveGroup', {
                            targetGroupId: destGroupId,
                            targetIndex: index,
                            groupId: groupId,
                            questionnaireId: questionnaireId
                        }
                    );
                };

                questionnaireService.moveQuestion = function(questionId, index, destGroupId, questionnaireId) {
                    return commandService.execute('MoveQuestion', {
                            targetGroupId: destGroupId,
                            targetIndex: index,
                            questionId: questionId,
                            questionnaireId: questionnaireId
                        }
                    );
                };

                questionnaireService.moveStaticText = function (entityId, index, destGroupId, questionnaireId) {
                    return commandService.execute('MoveStaticText', {
                        targetEntityId: destGroupId,
                        targetIndex: index,
                        entityId: entityId,
                        questionnaireId: questionnaireId
                    }
                    );
                };

                questionnaireService.removeItemWithId = function(items, itemId) {
                    var item = questionnaireService.findItem(items, itemId);
                    if (item) {
                        var parent = item.getParentItem();
                        var itemsToRemoveFrom = parent ? parent.items : items;
                        itemsToRemoveFrom.splice(_.indexOf(itemsToRemoveFrom, item), 1);
                    }
                };

                questionnaireService.findItem = function(items, itemId) {
                    var findFunc = function(item, itemToRemoveId) {
                        var itemToFind = _.findWhere(item.items, { itemId: itemId });
                        if (itemToFind) {
                            return itemToFind;
                        }

                        var childItems = item.items || [];
                        for (var i = 0; i < childItems.length; i++) {
                            var resultLocal = findFunc(childItems[i], itemToRemoveId);
                            if (resultLocal)
                                return resultLocal;
                        }

                        return null;
                    };

                    for (var j = 0; j < items.length; j++) {
                        var item = items[j];
                        if (item.itemId === itemId) return item;

                        var resultItem = findFunc(item, itemId);
                        if (resultItem) {
                            return resultItem;
                        }
                    }

                    return null;
                };

                return questionnaireService;
            }
        ]);

}());