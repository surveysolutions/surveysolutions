angular.module('designerApp')
    .controller('addClassificationCtrl',
        function ($rootScope, $scope, $http, $state, commandService, utilityService, isReadOnlyForUser) {
            var baseUrl = '../../api/findReplace';

            $scope.isReadOnlyForUser = isReadOnlyForUser;
           
            utilityService.setFocusIn('searchFor');

            $scope.loadClassificationGroups = function()
            {
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
                    })
                    .then(function(response) {
                        var newParams = $state.params;
                        newParams.property = null;
                        $state.go($state.current
                            .name,
                            newParams,
                            { notify: false, reload: false }); // reset state from previous search
                        $scope.foundReferences = response.data;
                        indexOfCurrentReference = -1;
                    });
            };

            $scope.loadClassificationGroups();
        });
