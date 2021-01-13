angular.module('designerApp')
    .factory('webTesterService',
        function ($http) {
            var webTesterService = {};

            webTesterService.run = function (questionnaireId, questionnaire, scenarioId) {
                var webTesterWindow = window.open("about:blank", '_blank');

                return $http.get('../../api/questionnaire/webTest/' + questionnaireId).then(function (response) {
                    webTesterService.setLocation(webTesterWindow, response, scenarioId);
                });
            };

            webTesterService.setLocation = function (webTesterWindow, response, scenarioId) {
                var url = response.data;
                if (!angular.isUndefined(scenarioId)) {
                    url += "?scenarioId=" + scenarioId;
                }
                webTesterWindow.location.href = url;
            };

            webTesterService.getScenarioSteps = function (questionnaireId, scenarioId) {
                return $http.get('../../api/questionnaire/' + questionnaireId + '/scenarios/' + scenarioId);
            };

            return webTesterService;
        }
    );
