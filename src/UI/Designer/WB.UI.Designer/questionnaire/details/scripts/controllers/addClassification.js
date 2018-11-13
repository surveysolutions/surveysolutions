angular.module('designerApp')
    .controller('addClassificationCtrl',
        function ($rootScope, $scope, $http, $state, commandService, utilityService, isReadOnlyForUser, $i18next) {
            var baseUrl = '../../api/classifications';

            $scope.isReadOnlyForUser = isReadOnlyForUser;
            var allClassificationGroups = { id: null, title: $i18next.t('AllClassifications') };
            $scope.groups = [];
            $scope.selectedGroup = allClassificationGroups;
            $scope.searchText = '';
            $scope.classifications = [];

            utilityService.setFocusIn('searchFor');

            $scope.loadClassificationGroups = function()
            {
                return $http({
                        method: 'GET',
                        url: baseUrl + '/groups',
                        params: {}
                    })
                    .then(function(response) {
                        $scope.groups = response.data;
                        $scope.groups.splice(0, 0, allClassificationGroups);
                    });
            };


            var search = function($scope) {
                $scope.$apply(function() {
                    var searchText = $scope.searchText.toLowerCase();
                    return $http({
                            method: 'GET',
                            url: baseUrl + '/search',
                            params: {
                                query: searchText,
                                groupId: $scope.selectedGroup.id
                            }
                        })
                        .then(function(response) {
                            $scope.classifications = response.data;
                        });
                });
            };

            var searchThrottled = _.debounce(search, 1000);

            $scope.$watch('searchText', function(){searchThrottled($scope);});

            $scope.changeClassificationGroup = function(group) {
                $scope.selectedGroup = group;
                searchThrottled($scope);
            };
            $scope.loadClassificationGroups();
        });
