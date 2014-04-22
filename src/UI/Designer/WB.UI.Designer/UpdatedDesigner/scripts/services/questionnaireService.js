angular.module('designerApp')
    .factory('questionnaireService', ['$http', function ($http) {

        var urlBase = 'api/questionnaire';
        var questionnaireService = {};

        questionnaireService.getQuestionnaireById = function (questionnaireId) {
            return $http.get(urlBase + '/get/' + questionnaireId);
        };

        questionnaireService.getChapterById = function (questionnaireId, chapterId) {
            return $http.get(urlBase + '/chapter/' + questionnaireId + "?chapterId=" + chapterId);
        };

        return questionnaireService;
    }]);