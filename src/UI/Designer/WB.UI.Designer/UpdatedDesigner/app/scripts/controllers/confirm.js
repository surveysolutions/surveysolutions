(function() {
    'use strict';
    angular.module('designerApp').controller('confirmCtrl',
    [
        '$scope', '$log', '$uibModalInstance', 'item',
        function ($scope, $log, $modalInstance, item) {
            $scope.item = item;

            $scope.ok = function() {
                $modalInstance.close('ok');
            };

            $scope.cancel = function() {
                $modalInstance.dismiss('cancel');
            };
        }
    ]);
})();