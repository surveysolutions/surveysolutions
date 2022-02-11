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

            pdfService.retryExportPdf = function(questionnaireId, translationId) {
                return $http({
                    method: 'POST',
                    url: '../../pdf/retry/' + questionnaireId,
                    data: {translation: translationId},
                    headers: { 'Content-Type': 'application/json', 'Accept': 'application/json', 'X-CSRF-TOKEN': getCsrfCookie() }
                });
            };

            return pdfService;
        }
    ]);
})();
