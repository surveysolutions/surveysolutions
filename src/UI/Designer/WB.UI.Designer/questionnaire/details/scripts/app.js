(function ($, window) {
    'use strict';
    var localizationInitPromise = $.Deferred();
    window.i18next
        .use(window.i18nextXHRBackend);
    window.i18next.init({
        debug: false,
        lng: $('html').attr('lang'), 
        fallbackLng: 'en', 
        backend: {
            loadPath: function(languages) {
                var key = 'QuestionnaireEditor.' + languages[0] + '.json'
                return '../build/resources/' + window.localization[key]
            }
        },
        load: 'languageOnly',
        useCookie: false,
        useLocalStorage: false,
        interpolation: {
            format: function(value, format, lng) {
                if (format === 'uppercase') 
                    return value.toUpperCase();
                if(moment.isDate(value) || moment.isMoment(value)) 
                    return moment(value).format(format);
                return value;
            }
        }
    }, function (err, t) {
        localizationInitPromise.resolve();
    });
    window.i18next.on('languageChanged', function(lng) {
        moment.locale(lng);
    });

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
        'angularMoment',
        'jm.i18next'
    ]);

    angular.module('designerApp').config(['$stateProvider', '$urlRouterProvider', '$rootScopeProvider', '$locationProvider',
        function ($stateProvider, $urlRouterProvider, $rootScopeProvider, $locationProvider) {

        $rootScopeProvider.digestTtl(12);

        $stateProvider
            .state('questionnaire',
            {
                url: "/{questionnaireId}",
                templateUrl: "views/main.v1.html",
                controller: 'MainCtrl',
                resolve: {
                    globalization: function() {
                        return localizationInitPromise.promise();
                    }
                }
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
                    },
                    'comments': {
                        templateUrl: 'views/comments.html',
                        controller: 'CommentsEditorCtrl'
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
                    },
                    'comments': {
                        templateUrl: 'views/comments.html',
                        controller: 'CommentsEditorCtrl'
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
                    },
                    'comments': {
                        templateUrl: 'views/comments.html',
                        controller: 'CommentsEditorCtrl'
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
                    },
                    'comments': {
                        templateUrl: 'views/comments.html',
                        controller: 'CommentsEditorCtrl'
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
                    },
                    'comments': {
                        templateUrl: 'views/comments.html',
                        controller: 'CommentsEditorCtrl'
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
        blockUiConfig.template = '<div class=\"block-ui-overlay\"></div><div class=\"block-ui-message-container\" aria-live=\"assertive\" aria-atomic=\"true\"><div class=\"block-ui-message\" ng-class=\"$_blockUiMessageClass\">{{ "PleaseWait" | i18next }}</div></div>';
        blockUiConfig.autoBlock = false;
    }])
    .config(['unsavedWarningsConfigProvider', function (unsavedWarningsConfigProvider) {
        unsavedWarningsConfigProvider.routeEvent = '$stateChangeStart';
        unsavedWarningsConfigProvider.navigateMessage = 'UnsavedChangesLeave';
        unsavedWarningsConfigProvider.reloadMessage = 'UnsavedChangesReload';
    }])
    .config(['cfpLoadingBarProvider', function (cfpLoadingBarProvider) {
        cfpLoadingBarProvider.spinnerTemplate = '<div id="loading-logo"></div>';
    }])
    .config(['$qProvider', function ($qProvider) {
        $qProvider.errorOnUnhandledRejections(false);
    }])
    .config(['hotkeysProvider', function(hotkeysProvider) {
        hotkeysProvider.cheatSheetDescription = 'HotkeysShowHideHelp';
        hotkeysProvider.templateTitle = 'HotkeysShortcuts';
        hotkeysProvider.template = '<div class="cfp-hotkeys-container fade" ng-class="{in: helpVisible}" style="display: none;"><div class="cfp-hotkeys">' +
                          '<h4 class="cfp-hotkeys-title" ng-if="!header">{{ title | i18next}}</h4>' +
                          '<div ng-bind-html="header" ng-if="header"></div>' +
                          '<table><tbody>' +
                            '<tr ng-repeat="hotkey in hotkeys | filter:{ description: \'!$$undefined$$\' }">' +
                              '<td class="cfp-hotkeys-keys">' +
                                '<span ng-repeat="key in hotkey.format() track by $index" class="cfp-hotkeys-key">{{ key }}</span>' +
                              '</td>' +
                              '<td class="cfp-hotkeys-text">{{ hotkey.description | i18next }}</td>' +
                            '</tr>' +
                          '</tbody></table>' +
                          '<div ng-bind-html="footer" ng-if="footer"></div>' +
                          '<div class="cfp-hotkeys-close" ng-click="toggleCheatSheet()">&#215;</div>' +
                        '</div></div>';
    }]);

    angular.module('templates', []);

})(jQuery, window);
