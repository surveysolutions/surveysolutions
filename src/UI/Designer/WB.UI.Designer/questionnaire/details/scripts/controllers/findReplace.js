angular.module('designerApp')
    .controller('findReplaceCtrl',
        function ($rootScope, $scope, $http, $state, commandService, utilityService, isReadOnlyForUser) {
            var baseUrl = '../../api/findReplace';

            $scope.isReadOnlyForUser = isReadOnlyForUser;
            $scope.searchForm = {
                searchFor: '',
                replaceWith: '',
                matchCase: false,
                matchWholeWord: false,
                useRegex: false
            };

            $scope.foundReferences = [];
            $scope.step = 'search';

            utilityService.setFocusIn('searchFor');

            var indexOfCurrentReference = -1;

            $scope.$watch('searchForm.searchFor',
                function() {
                    if ($scope.foundReferences.length) {
                        $scope.foundReferences.splice(0, $scope.foundReferences.length);
                    }
                });

            $scope.findAll = function () {
                if ($scope.searchForm.searchFor.length > 0) {
                    return $http({
                            method: 'GET',
                            url: baseUrl + '/findAll',
                            params: {
                                searchFor: $scope.searchForm.searchFor,
                                matchCase: $scope.searchForm.matchCase,
                                matchWholeWord: $scope.searchForm.matchWholeWord,
                                useRegex: $scope.searchForm.useRegex,
                                id: $state.params.questionnaireId
                            }
                        })
                        .then(function(response) {
                            var newParams = $state.params;
                            newParams.property = null;
                            $state.go($state.current
                                .name,
                                newParams,
                                { notify: false, reload: false }); // reset state from previous search
                            $scope.foundReferences = response.data;
                            indexOfCurrentReference = -1;
                        });
                }
            }

            $scope.confirmReplaceAll = function() {
                $scope.step = 'confirm';
            };

            $scope.replaceAll = function() {
                commandService
                    .execute('ReplaceTexts',
                    {
                        questionnaireId: $state.params.questionnaireId,
                        searchFor: $scope.searchForm.searchFor,
                        replaceWith: $scope.searchForm.replaceWith,
                        matchCase: $scope.searchForm.matchCase,
                        matchWholeWord: $scope.searchForm.matchWholeWord,
                        useRegex: $scope.searchForm.useRegex
                    })
                    .then(function() {
                        $scope.step = 'done';
                    });
            };

            $scope.onDone = function() {
                $scope.$close();
                $state.reload();
            };

            $scope.backToSearch = function() {
                $scope.step = 'search';
                $scope.foundReferences.splice(0, $scope.foundReferences.length);
            };

            $scope.navigateNext = function() {
                indexOfCurrentReference++;
                if (indexOfCurrentReference >= $scope.foundReferences.length) {
                    indexOfCurrentReference = 0;
                }
                $rootScope.navigateTo($scope.foundReferences[indexOfCurrentReference], true);
            };

            $scope.navigatePrev = function() {
                indexOfCurrentReference--;
                if (indexOfCurrentReference < 0) {
                    indexOfCurrentReference = $scope.foundReferences.length - 1;
                }
                $rootScope.navigateTo($scope.foundReferences[indexOfCurrentReference], true);
            };

            $rootScope.$on('openMacrosList',
                function() {
                    angular.element('.findReplaceModal').css('left', "50%");
                });

            $rootScope.$on('$stateChangeSuccess',
                function() {
                    angular.element('.findReplaceModal').css('left', "");
                });
        });
