angular.module('designerApp')
    .controller('findReplaceCtrl', function ($rootScope, $scope, $http, $state) {
        var baseUrl = '../../api/findReplace';

        $scope.searchFor = '';
        $scope.relaceWith = '';
        var indexOfCurrentReference = -1;
        $scope.foundReferences = [];

        $scope.findAll = function() {
            return $http({
                method: 'GET',
                url: baseUrl + '/findAll',
                params: {
                    searchFor: $scope.searchFor,
                    id: $state.params.questionnaireId
                },
                headers: { 'Content-Type': 'application/json', 'Accept': 'application/json' }
            }).then(function(response) {
                $scope.foundReferences = response.data;
                $scope.indexOfCurrentReference = -1;
            });
        }
        $scope.navigateNext = function() {
            indexOfCurrentReference++;
            if (indexOfCurrentReference > $scope.foundReferences.length) {
                indexOfCurrentReference = 0; 
            } else {
                $rootScope.navigateTo($scope.foundReferences[indexOfCurrentReference]);
            }
        };
        $scope.navigatePrev = function() {
            indexOfCurrentReference--;
            if (indexOfCurrentReference < 0) {
                indexOfCurrentReference = 0;
            }
            $rootScope.navigateTo($scope.foundReferences[indexOfCurrentReference]);
        };
    });