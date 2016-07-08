(function() {
    angular.module('designerApp')
        .factory('verificationService', [
            '$http', function($http) {
                var verificationService = {};

                verificationService.verify = function(questionnaireId) {
                    return $http.get('../../api/questionnaire/verify/' + questionnaireId);
                };

                return verificationService;
            }
        ]);
})();