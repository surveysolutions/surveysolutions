﻿angular.module('designerApp')
    .factory('webTesterService',
        function ($http) {
            var webTesterService = {};

            webTesterService.run = function (questionnaireId, questionnaire, scenarioId) {
                var webTesterWindow = window.open("about:blank", '_blank');

                return $http.get('../../api/questionnaire/webTest/' + questionnaireId).then(function (response) {
                    webTesterService.setLocation(webTesterWindow, response, !questionnaire.isReadOnlyForUser, scenarioId);
                });
            };

            webTesterService.setLocation = function (webTesterWindow, response, saveScenarioAvailable, scenarioId) {
                var url = response.data + "?saveScenarioAvailable=" + saveScenarioAvailable;
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
