(function() {
    angular.module('designerApp').factory('pdfService', [
        '$http', 'utilityService', 'commandService',
        function($http, utils, commandService) {
            var pdfService = {};

            pdfService.updateExportPdfStatus = function(questionnaireId, translationId) {
                return $http.get('../../pdf/status/' +
                    questionnaireId +
                    '?timezoneOffsetMinutes=' +
                    new Date().getTimezoneOffset() +
                    '&translation=' +
                    translationId);
            };

            return pdfService;
        }
    ]);
})();
