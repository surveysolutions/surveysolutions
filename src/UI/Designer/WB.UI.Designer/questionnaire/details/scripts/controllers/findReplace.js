angular.module('designerApp')
    .controller('findReplaceCtrl', function ($rootScope, $scope, $http, $state, commandService, confirmService) {
        var baseUrl = '../../api/findReplace';

        $scope.searchFor = '';
        $scope.replaceWith = '';
        $scope.matchCase = false;
        $scope.matchWholeWord = false;
        $scope.useRegex = false;
        $scope.foundReferences = [];

        var indexOfCurrentReference = -1;
        $scope.findAll = function () {
            return $http({
                method: 'GET',
                url: baseUrl + '/findAll',
                params: {
                    searchFor: $scope.searchFor,
                    matchCase: $scope.matchCase,
                    matchWholeWord: $scope.matchWholeWord,
                    useRegex: $scope.useRegex,
                    id: $state.params.questionnaireId
                }
            }).then(function(response) {
                $scope.foundReferences = response.data;
                $scope.indexOfCurrentReference = -1;
            });
        }

        $scope.replaceAll = function () {
            var confirmModal = confirmService.open({
                title: "Replace '" + $scope.searchFor + "' with '" + $scope.replaceWith + "' in all found items?",
                okButtonTitle: "Replace",
                cancelButtonTitle: "Back to search"
            });
            confirmModal.result.then(function(confirmResult) {
                if (confirmResult === 'ok') {
                    commandService
                        .execute('ReplaceTexts',
                        {
                            questionnaireId: $state.params.questionnaireId,
                            searchFor: $scope.searchFor,
                            replaceWith: $scope.replaceWith,
                            matchCase: $scope.matchCase,
                            matchWholeWord: $scope.matchWholeWord,
                            useRegex: $scope.useRegex
                        })
                        .then(function() {
                            return $scope.findAll();
                        });
                }
            });
        }

        $scope.navigateNext = function() {
            indexOfCurrentReference++;
            if (indexOfCurrentReference >= $scope.foundReferences.length) {
                indexOfCurrentReference = 0; 
            }

            $rootScope.navigateTo($scope.foundReferences[indexOfCurrentReference]);
        };
        $scope.navigatePrev = function() {
            indexOfCurrentReference--;
            if (indexOfCurrentReference < 0) {
                indexOfCurrentReference = 0;
            }
            $rootScope.navigateTo($scope.foundReferences[indexOfCurrentReference]);
        };
    });