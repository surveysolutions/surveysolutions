angular.module('questionnaires', ['ui.bootstrap'])
    .controller('importController', ['$scope', '$modal', '$log', function ($scope, $modal, $log) {

        $scope.open = function () {

            var modalInstance = $modal.open({
                templateUrl: 'myModalContent.html',
                controller: 'importDialogController',
                resolve: {
                }
            });

            modalInstance.result.then(function () {
                $log.info('Model closed');
            }, function () {
                $log.info('Modal dismissed at: ' + new Date());
            });
        };
    }])
    .controller('importDialogController', ['$scope', '$modalInstance', '$http', '$log', function ($scope, $modalInstance, $http, $log) {
        $scope.signInVisible = true;
        $scope.loading = false;
        $scope.credentials = {
            userName: '',
            password: ''
        };

        $scope.questionnaires = {
            total: 0,
            selectedItemId: '',
            items: []
        }

        $scope.next = function () {

            $scope.loading = true;

            $http({
                method: 'POST',
                url: '/Headquarters/api/Questionnaires/LoginToDesigner',
                params: {
                    userName: $scope.credentials.userName,
                    password: $scope.credentials.password
                }
            }).success(function () {
                $scope.listToImportVisible = true;
                $scope.signInVisible = false;
                $scope.listToImportVisible = true;

                $http({
                    method: 'GET',
                    url: '/Headquarters/api/questionnaires',
                    params: {
                        filter: ''
                    }
                }).success(function (data) {
                    $scope.questionnaires.items = data.Items;
                    $scope.questionnaires.total = data.Total;
                });

                $scope.loading = false;

            }).error(function (data) {
                $scope.loading = false;
                $scope.errorMessage = data.Message;
                $log.error(data);
            });
        };

        $scope.selectItem = function (itemId) {
            $scope.questionnaires.selectedItemId = itemId;
            $log.info(itemId);
        }

        $scope.cancel = function () {
            $modalInstance.dismiss('cancel');
        };
    }]);