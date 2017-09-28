(function ($) {
    'use strict';

    angular.module('designerApp', [
        'ngCookies',
        'ngSanitize',
        'ngResource',
        'ngAnimate',
        'ui.bootstrap',
        'ui.bootstrap.tpls',
        'ui.tree',
        'ui.highlight',
        'jlareau.pnotify',
        'ui.router',
        'angular-loading-bar',
        'cfp.hotkeys',
        'blockUI',
        'unsavedChanges',
        'monospaced.elastic',
        'perfect_scrollbar',
        'ng-context-menu',
        'ui.ace',
        'templates',
        'ngFileUpload',
        'angularMoment'
    ]);

    angular.module('designerApp').config(['$stateProvider', '$urlRouterProvider', '$rootScopeProvider', '$locationProvider', function ($stateProvider, $urlRouterProvider, $rootScopeProvider, $locationProvider) {

        $rootScopeProvider.digestTtl(12);

        $stateProvider
            .state('questionnaire',
            {
                url: "/{questionnaireId}",
                templateUrl: "views/main.v1.html",
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
                params: {
                    property: 'None',
                    indexOfEntityInProperty: null
                },
                views: {
                    '': {
                        templateUrl: 'views/question.html',
                        controller: 'QuestionCtrl'
                    }
                }
            }).state('questionnaire.chapter.group', {
                url: "/group/{itemId}",
                params: {
                    property: 'None',
                    indexOfEntityInProperty: null
                },
                views: {
                    '': {
                        templateUrl: 'views/group.html',
                        controller: 'GroupCtrl'
                    }
                }
            }).state('questionnaire.chapter.variable', {
                url: "/variable/{itemId}",
                params: {
                    property: 'None',
                    indexOfEntityInProperty: null
                },
                views: {
                    '': {
                        templateUrl: 'views/variable.html',
                        controller: 'VariableCtrl'
                    }
                }
            }).state('questionnaire.chapter.roster', {
                url: "/roster/{itemId}",
                params: {
                    property: 'None',
                    indexOfEntityInProperty: null
                },
                views: {
                    '': {
                        templateUrl: 'views/roster.html',
                        controller: 'RosterCtrl'
                    }
                }
            }).state('questionnaire.chapter.statictext', {
                url: "/static-text/{itemId}",
                params: {
                    property: 'None',
                    indexOfEntityInProperty: null
                },
                views: {
                    '': {
                        templateUrl: 'views/static-text.v1.html',
                        controller: 'StaticTextCtrl'
                    }
                }
            });

        $locationProvider.html5Mode({
            enabled: true,
            requireBase: false
        });

    }])
    .config(['$httpProvider', function ($httpProvider) {
        $httpProvider.interceptors.push('errorReportingInterceptor');
        $httpProvider.interceptors.push('authorizationInterceptor');
    }])
    .config(['blockUIConfig', function (blockUiConfig) {
        blockUiConfig.message = 'Please wait...';
        blockUiConfig.autoBlock = false;
    }])
    .config(['unsavedWarningsConfigProvider', function (unsavedWarningsConfigProvider) {
        unsavedWarningsConfigProvider.routeEvent = '$stateChangeStart';
    }])
    .config(['cfpLoadingBarProvider', function (cfpLoadingBarProvider) {
        cfpLoadingBarProvider.spinnerTemplate = '<div id="loading-logo"></div>';
    }])
    .config(['$qProvider', function ($qProvider) {
        $qProvider.errorOnUnhandledRejections(false);
    }]);

    angular.module('templates', []);

}(jQuery));