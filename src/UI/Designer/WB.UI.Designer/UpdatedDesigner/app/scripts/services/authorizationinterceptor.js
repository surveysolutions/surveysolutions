angular.module('designerApp')
    .factory('authorizationInterceptor', [
        '$q',
        function($q) {
            'use strict';
            return {
                'responseError': function (rejection) {
                    if (rejection.status == 401 || rejection.status == 403) {
                        //alert('Your session expired. Please relogin.');
                        window.location.href = '../../';
                    }
                    return $q.reject(rejection);
                }
            };
        }
    ]);
