(function() {
    'use strict';
    angular.module('designerApp')
        .controller('confirmCtrl',
            function ($scope, $log, $uibModalInstance, item) {
                $scope.item = item;

                $scope.ok = function() {
                    $uibModalInstance.close('ok');
                };

                $scope.cancel = function() {
                    $uibModalInstance.dismiss('cancel');
                };
            }
        );
})();