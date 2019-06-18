angular.module('designerApp')
    .factory('webTesterService',
        function ($http) {
            var webTesterService = {};

            webTesterService.run = function (questionnaireId) {
                return $http.get('../../api/questionnaire/webTest/' + questionnaireId);
            };

            webTesterService.setLocation = function (webTesterWindow, response, isAdmin, scenarioId) {
                var url = response.data + "?saveScenarioAvailable=" + isAdmin;
                if (!angular.isUndefined(scenarioId)) {
                    url += "&scenarioId=" + scenarioId;
                }
                webTesterWindow.location.href = url;
            };

            webTesterService.getScenarioSteps = function (questionnaireId, scenarioId) {
                return $http.get('../../api/questionnaire/' + questionnaireId + '/scenarios/' + scenarioId);
            };

            return webTesterService;
        }
    );
