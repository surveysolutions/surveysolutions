(function() {
    angular.module('designerApp')
        .factory('errorReportingInterceptor', [
            '$q', 'notificationService',
            function ($q, notificationService) {
                return {
                    response: function (response) {
                        if (response.data.IsSuccess === false) {
                            notificationService.notice(response.data.Error);
                        }
                        return response;
                    },
                    responseError: function (rejection) {
                        notificationService.error('Request failed unexpectedly.');
                        return $q.reject(rejection);
                    }
                }
        }]);
})();