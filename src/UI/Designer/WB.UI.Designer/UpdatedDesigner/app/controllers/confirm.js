(function() {
    'use strict';
    angular.module('designerApp').controller('confirmCtrl',
    [
        '$scope', '$log', '$modalInstance',
        function($scope, $log, $modalInstance) {
            $scope.ok = function() {
                $modalInstance.close('ok');
            };

            $scope.cancel = function() {
                $modalInstance.dismiss('cancel');
            };
        }
    ]);
})();