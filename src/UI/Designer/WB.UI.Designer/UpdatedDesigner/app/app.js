'use strict';

var app = angular.module('designerApp', [
        'ngCookies',
        'ngResource',
        'ngSanitize',
        'ngRoute',
        'angular-underscore'
    ])
    .config(function($routeProvider) {
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
            .when('/:questionnaireId/editchapter/:chapterId', {
                templateUrl: 'app/views/main.html',
                controller: 'MainCtrl',
                reloadOnSearch: false
            })
            .otherwise({
                redirectTo: '/'
            });
    });