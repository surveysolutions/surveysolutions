angular.module('designerApp')
    .factory('errorReportingInterceptor', [
        '$q', 'notificationService', '$i18next',
        function ($q, notificationService, $i18next) {
            return {
                responseError: function (rejection) {
                    if (rejection.status === 406 || rejection.status === 403 || rejection.status === 400) {
                        if (rejection.data.message) {
                            notificationService.notice(rejection.data.message);
                        } else {
                            notificationService.notice(rejection.data.Message);
                        }
                    }
                    else if (rejection.status === 404) {
                        notificationService.notice($i18next.t("EntryWasNotFound"));
                    } else {
                        notificationService.error($i18next.t("RequestFailedUnexpectedly"));
                    }
                    return $q.reject(rejection);
                }
            };
        }]);
