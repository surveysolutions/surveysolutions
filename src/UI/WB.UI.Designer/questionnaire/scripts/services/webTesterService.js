(function() {
    angular.module('designerApp')
        .factory('webTesterService', [
            '$http', function($http) {
                var webTesterService = {};

                webTesterService.run = function(questionnaireId) {
                    return $http.get('../../api/questionnaire/webTest/' + questionnaireId);
                };

                webTesterService.getScenarioSteps = function(questionnaireId, scenarioId){
                    return $http.get('../../api/questionnaire/'+ questionnaireId + '/scenarios/' + scenarioId);
                }

                return webTesterService;
            }
        ]);
})();