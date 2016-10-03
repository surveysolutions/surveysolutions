angular.module('designerApp')
    .controller('findReplaceCtrl', function ($scope) {
        $scope.searchFor = '';
        $scope.relaceWith = '';

        $scope.foundReferences = [];
    });