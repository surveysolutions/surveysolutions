(function ($) {
    'use strict';

    angular.module('designerApp', [
        'ngCookies',
        'ngSanitize',
        'ngResource',
        'ngAnimate',
        'ui.bootstrap',
        'ui.bootstrap.tpls',
        'ui.bootstrap.transition',
        'ui.tree',
        'ui.utils',
        'ui.notify',
        'ui.router',
        'angular-loading-bar',
        'cfp.hotkeys',
        'blockUI',
        'unsavedChanges',
        'monospaced.elastic'
    ]);

    angular.module('designerApp').config(['$stateProvider', '$urlRouterProvider', function ($stateProvider, $urlRouterProvider) {
        var questionnaireId = $.cookie('questionnaireId');
        var url = '/' + questionnaireId;
        $urlRouterProvider.otherwise(url);

        $stateProvider
            .state('questionnaire', {
                url: "/{questionnaireId}",
                templateUrl: "views/main.html",
                controller: 'MainCtrl'
            }).state('questionnaire.chapter', {
                url: "/chapter/{chapterId}",
                views: {
                    '': {
                        templateUrl: "views/tree.html",
                        controller: 'TreeCtrl'
                    }
                }
                
            }).state('questionnaire.chapter.question', {
                url: "/question/{itemId}",
                views: {
                    '': {
                        templateUrl: 'views/question.html',
                        controller: 'QuestionCtrl'
                    }
                }
            }).state('questionnaire.chapter.group', {
                url: "/group/{itemId}",
                views: {
                    '': {
                        templateUrl: 'views/group.html',
                        controller: 'GroupCtrl'
                    }
                }
            }).state('questionnaire.chapter.roster', {
                url: "/roster/{itemId}",
                views: {
                    '': {
                        templateUrl: 'views/roster.html',
                        controller: 'RosterCtrl'
                    }
                }
            }).state('questionnaire.chapter.statictext', {
                url: "/static-text/{itemId}",
                views: {
                    '': {
                        templateUrl: 'views/static-text.html',
                        controller: 'StaticTextCtrl'
                    }
                }
            });
    }])
    .config(['$httpProvider', function ($httpProvider) {
        $httpProvider.interceptors.push('errorReportingInterceptor');
        $httpProvider.interceptors.push('authorizationInterceptor');
    }])
    .config(['blockUIConfigProvider', function(blockUiConfigProvider) {
        blockUiConfigProvider.message('Please wait...');
        blockUiConfigProvider.autoBlock(false);
    }])
    .config(['unsavedWarningsConfigProvider', function(unsavedWarningsConfigProvider) {
        unsavedWarningsConfigProvider.routeEvent = '$stateChangeStart';
    }]);
}(jQuery));