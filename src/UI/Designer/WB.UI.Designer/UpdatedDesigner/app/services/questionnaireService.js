(function() {
    'use strict';
    angular.module('designerApp')
        .factory('questionnaireService', [
            '$http', 'utilityService',
            function($http, string) {

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

                return questionnaireService;
            }
        ]);

}());