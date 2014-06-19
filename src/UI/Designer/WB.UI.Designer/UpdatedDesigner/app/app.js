(function() {
    'use strict';

    angular.module('designerApp', [
        'ngCookies',
        'ngResource',
        'ngSanitize',
        'ngRoute',
        'angular-underscore',
        'ui.bootstrap',
        'ui.bootstrap.tpls',
        'ui.bootstrap.transition',
        'ui.tree',
        'ui.utils',
        'angular-loading-bar',
        'cfp.hotkeys'
    ]);

    angular.module('designerApp').config([
        '$routeProvider', function($routeProvider) {
            $routeProvider
                .when('/:questionnaireId', {
                    templateUrl: 'app/views/main.html',
                    controller: 'MainCtrl'
                })
                .when('/:questionnaireId/chapter/:chapterId', {
                    templateUrl: 'app/views/main.html',
                    controller: 'MainCtrl',
                    reloadOnSearch: false
                })
                .when('/:questionnaireId/chapter/:chapterId/item/:itemId', {
                    templateUrl: 'app/views/main.html',
                    controller: 'MainCtrl',
                    reloadOnSearch: false
                })
                .when('/:questionnaireId/chapter/:chapterId/question/:itemId', {
                    templateUrl: 'app/views/main.html',
                    controller: 'MainCtrl',
                    reloadOnSearch: false
                })
                .when('/:questionnaireId/chapter/:chapterId/chapter/:itemId', {
                    templateUrl: 'app/views/main.html',
                    controller: 'MainCtrl',
                    reloadOnSearch: false
                })
                .when('/:questionnaireId/chapter/:chapterId/roster/:itemId', {
                    templateUrl: 'app/views/main.html',
                    controller: 'MainCtrl',
                    reloadOnSearch: false
                })
                .when('/:questionnaireId/chapter/:chapterId/group/:itemId', {
                    templateUrl: 'app/views/main.html',
                    controller: 'MainCtrl',
                    reloadOnSearch: false
                })
                .otherwise({
                    redirectTo: '/'
                });
        }
    ]);
}());