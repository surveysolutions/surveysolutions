angular.module('designerApp')
    .controller('findReplaceCtrl', function ($rootScope, $scope, $http, $state, commandService, confirmService) {
        var baseUrl = '../../api/findReplace';

        $scope.searchForm = {
            searchFor: '',
            replaceWith: '',
            matchCase: false,
            matchWholeWord: false,
            useRegex: false
        };

        $scope.foundReferences = [];
        $scope.step = 'search';

        var indexOfCurrentReference = -1;

        $scope.$watch('searchForm.searchFor', function (newValue) {
            if ($scope.foundReferences.length) {
                $scope.foundReferences.splice(0, $scope.foundReferences.length);
            }
        });

        $scope.findAll = function () {
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
            }).then(function (response) {
                $scope.foundReferences = response.data;
                indexOfCurrentReference = -1;
            });
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

        $scope.backToSearch = function() {
            $scope.step = 'search';
            $scope.foundReferences.splice(0, $scope.foundReferences.length);
        };

        $scope.navigateNext = function () {
            indexOfCurrentReference++;
            if (indexOfCurrentReference >= $scope.foundReferences.length) {
                indexOfCurrentReference = 0;
            }
            console.info(indexOfCurrentReference);
            $rootScope.navigateTo($scope.foundReferences[indexOfCurrentReference]);
        };

        $scope.navigatePrev = function () {
            indexOfCurrentReference--;
            if (indexOfCurrentReference < 0) {
                indexOfCurrentReference = $scope.foundReferences.length - 1;
            }
            $rootScope.navigateTo($scope.foundReferences[indexOfCurrentReference]);
        };
    });