angular.module('designerApp')
    .factory('authorizationInterceptor', [
        '$q',
        function($q) {
            'use strict';
            return {
                'responseError': function (rejection) {
                    if (rejection.status == 401) {
                        window.location.href = '/';
                    }
                    return $q.reject(rejection);
                }
            };
        }
    ]);
