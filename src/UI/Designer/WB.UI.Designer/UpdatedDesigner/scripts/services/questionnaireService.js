angular.module('designerApp')
    .factory('questionnaireService', [
        '$http', 'utilityService',
        function ($http, string) {

        var urlBase = 'api/questionnaire';
        var questionnaireService = {};

        questionnaireService.getQuestionnaireById = function (questionnaireId) {
            return $http.get(urlBase + '/get/' + questionnaireId);
        };

        questionnaireService.getChapterById = function (questionnaireId, chapterId) {
            return $http.get(urlBase + '/chapter/' + questionnaireId + "?chapterId=" + chapterId);
        };

        questionnaireService.getGroupEditForm = function (questionnaireId, groupId) {
            return $http.get(urlBase + '/EditGroup/' + questionnaireId + "?qroupId=" + groupId);
        };

        questionnaireService.getQuestionDetailsById = function (questionnaireId, questionId) {
            return $http.get(urlBase + '/editQuestion/' + questionnaireId + "?questionId=" + questionId);
        };

        return questionnaireService;
    }]);