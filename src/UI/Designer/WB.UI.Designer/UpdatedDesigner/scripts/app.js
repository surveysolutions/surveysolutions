'use strict';

angular.module('pocAngularApp', [
  'ngCookies',
  'ngResource',
  'ngSanitize',
  'ngRoute',
  'angular-underscore'
])
  .config(function ($routeProvider) {
    $routeProvider
      .when('/:questionnaireId', {
          templateUrl: 'UpdatedDesigner/views/main.html',
          controller: 'MainCtrl'
      })
      .when('/:questionnaireId/chapter/:chapterId', {
          templateUrl: 'UpdatedDesigner/views/main.html',
          controller: 'MainCtrl',
          reloadOnSearch: false
      })
      .when('/:questionnaireId/chapter/:chapterId/item/:itemId', {
          templateUrl: 'UpdatedDesigner/views/main.html',
          controller: 'MainCtrl',
          reloadOnSearch: false
      })
      .otherwise({
        redirectTo: '/'
      });
  });