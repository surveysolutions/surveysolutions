angular.module('designerApp')
    .controller('findReplaceCtrl', function ($scope, $http, $state) {
        var baseUrl = '../../api/findReplace';

        $scope.searchFor = '';
        $scope.relaceWith = '';
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
            });
        }
    });