(function ($) {
    'use strict';

    angular.module('designerApp', [
        'ngCookies',
        'ngResource',
        'ngSanitize',
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
        'blockUI'
    ]);

    angular.module('designerApp').config(['$stateProvider', '$urlRouterProvider', function ($stateProvider, $urlRouterProvider) {
        var questionnaireId = $.cookie('questionnaireId');
        var url = '/' + questionnaireId;
        $urlRouterProvider.otherwise(url);

        $stateProvider
            .state('questionnaire', {
                url: "/{questionnaireId}",
                templateUrl: "app/views/main.html",
                controller: 'MainCtrl'
            }).state('questionnaire.chapter', {
                url: "/chapter/{chapterId}",
                views: {
                    '': {
                        templateUrl: "app/views/tree.html",
                        controller: 'TreeCtrl',
                        resolve: {
                            questionnaireId: ['$stateParams', function($stateParams) {
                                return $stateParams.questionnaireId;
                            }],
                            itemId: ['$stateParams', function ($stateParams) {
                                return $stateParams.itemId;
                            }]
                        }
                    }
                }
                
            }).state('questionnaire.chapter.question', {
                url: "/question/{itemId}",
                views: {
                    '': {
                        templateUrl: 'app/views/question.html',
                        controller: 'QuestionCtrl',
                        resolve: {
                            questionnaireId: ['$stateParams', function ($stateParams) {
                                return $stateParams.questionnaireId;
                            }]
                        }
                    }
                }
            }).state('questionnaire.chapter.group', {
                url: "/group/{itemId}",
                views: {
                    '': {
                        templateUrl: 'app/views/group.html',
                        controller: 'GroupCtrl',
                        resolve: {
                            questionnaireId: ['$stateParams', function ($stateParams) {
                                return $stateParams.questionnaireId;
                            }]
                        }
                    }
                }
            }).state('questionnaire.chapter.roster', {
                url: "/roster/{itemId}",
                views: {
                    '': {
                        templateUrl: 'app/views/roster.html',
                        controller: 'RosterCtrl',
                        resolve: {
                            questionnaireId: ['$stateParams', function ($stateParams) {
                                return $stateParams.questionnaireId;
                            }]
                        }
                    }
                }
            });
    }]).config(['$httpProvider', function ($httpProvider) {
        $httpProvider.interceptors.push('errorReportingInterceptor');
    }])
    .config(['blockUIConfigProvider', function(blockUiConfigProvider) {
        blockUiConfigProvider.message('Please wait...');
    }]);
}(jQuery));