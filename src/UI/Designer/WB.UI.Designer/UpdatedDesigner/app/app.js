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
        'cfp.hotkeys'
    ]);

    //angular.module('designerApp').config([
    //    '$routeProvider', function($routeProvider) {
    //        $routeProvider
    //            .when('/:questionnaireId', {
    //                templateUrl: 'app/views/main.html',
    //                controller: 'MainCtrl'
    //            })
    //            .when('/:questionnaireId/chapter/:chapterId', {
    //                templateUrl: 'app/views/main.html',
    //                controller: 'MainCtrl',
    //                reloadOnSearch: false
    //            })
    //            .when('/:questionnaireId/chapter/:chapterId/item/:itemId', {
    //                templateUrl: 'app/views/main.html',
    //                controller: 'MainCtrl',
    //                reloadOnSearch: false
    //            })
    //            .when('/:questionnaireId/chapter/:chapterId/question/:itemId', {
    //                templateUrl: 'app/views/main.html',
    //                controller: 'MainCtrl',
    //                reloadOnSearch: false
    //            })
    //            .when('/:questionnaireId/chapter/:chapterId/chapter/:itemId', {
    //                templateUrl: 'app/views/main.html',
    //                controller: 'MainCtrl',
    //                reloadOnSearch: false
    //            })
    //            .when('/:questionnaireId/chapter/:chapterId/roster/:itemId', {
    //                templateUrl: 'app/views/main.html',
    //                controller: 'MainCtrl',
    //                reloadOnSearch: false
    //            })
    //            .when('/:questionnaireId/chapter/:chapterId/group/:itemId', {
    //                templateUrl: 'app/views/main.html',
    //                controller: 'MainCtrl',
    //                reloadOnSearch: false
    //            })
    //            .otherwise({
    //                redirectTo: '/'
    //            });
    //    }
    //]).run(['$location', '$cookies', 'utilityService', function ($location, $cookies, utilityService) {
    //    if (!$location.url()) {
    //        var questionnaireId = $cookies.questionnaireId;
    //        var url = utilityService.format('/{0}', questionnaireId);
    //        $location.path(url);
    //    }
    //}]);

    angular.module('designerApp').config(['$stateProvider', '$urlRouterProvider', function ($stateProvider, $urlRouterProvider) {
        var questionnaireId = $.cookie('questionnaireId');
        var url = '/' + questionnaireId;
        $urlRouterProvider.otherwise(url);

        $stateProvider.state('questionnaire', {
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
                templateUrl: "app/views/main.html",
                controller: 'MainCtrl'
            });
    }]).config(['$httpProvider', function ($httpProvider) {
        $httpProvider.interceptors.push('errorReportingInterceptor');
    }]);
}(jQuery));