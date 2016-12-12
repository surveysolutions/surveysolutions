angular.module('designerApp')
    .controller('LookupTablesCtrl',
        function ($rootScope, $scope, $state, hotkeys, commandService, utilityService, confirmService) {
            'use strict';

            $scope.downloadLookupFileBaseUrl = '../../Questionnaire/ExportLookupTable/';
            var hideLookupTablesPane = 'ctrl+l';

            if (hotkeys.get(hideLookupTablesPane) !== false) {
                hotkeys.del(hideLookupTablesPane);
            }

            hotkeys.add(hideLookupTablesPane, 'Close lookup tables panel', function (event) {
                event.preventDefault();
                $scope.foldback();
            });

            $scope.lookupTables = [];

            var dataBind = function (lookupTable, lookupTableDto) {
                lookupTable.initialLookupTable = angular.copy(lookupTableDto);

                lookupTable.itemId = lookupTableDto.itemId;
                lookupTable.name = lookupTableDto.name;
                lookupTable.fileName = lookupTableDto.fileName;
                lookupTable.file = null;
                lookupTable.hasUploadedFile = !_.isEmpty(lookupTableDto.fileName);
            };

            $scope.loadLookupTables = function () {
                if ($scope.questionnaire === null || $scope.questionnaire.lookupTables === null)
                    return;

                _.each($scope.questionnaire.lookupTables, function (lookupTableDto) {
                    var lookupTable = {};
                    if (!_.any($scope.lookupTables, function(elem) { return elem.itemId === lookupTableDto.itemId; }))
                    {
                        dataBind(lookupTable, lookupTableDto);
                        $scope.lookupTables.push(lookupTable);
                    }
                });
            };

            $scope.isFolded = false;

            $scope.unfold = function () {
                $scope.isFolded = true;
            };

            $scope.addNewLookupTable = function () {
                var newId = utilityService.guid();

                var newLookupTables = {
                    itemId: newId
                };

                commandService.addLookupTable($state.params.questionnaireId, newLookupTables).success(function () {
                    var lookupTables = {};
                    dataBind(lookupTables, newLookupTables);
                    $scope.lookupTables.push(lookupTables);
                });
            };
            $scope.fileSelected = function (lookupTable, file) {
                if (_.isUndefined(file) || _.isNull(file)) {
                    return;
                }
                lookupTable.file = file;
                lookupTable.fileName = lookupTable.file.name;

                lookupTable.oldItemId = lookupTable.itemId;
                lookupTable.itemId = utilityService.guid();

                lookupTable.form.$setDirty();
            }
            $scope.saveLookupTable = function (lookupTable) {

                commandService.updateLookupTable($state.params.questionnaireId, lookupTable).success(function() {
                    lookupTable.initialLookupTable = angular.copy(lookupTable);
                    lookupTable.hasUploadedFile = !_.isEmpty(lookupTable.fileName);
                    lookupTable.form.$setPristine();
                });
            };

            $scope.cancel = function(lookupTable) {
                var temp = angular.copy(lookupTable.initialLookupTable);
                dataBind(lookupTable, temp);
                lookupTable.form.$setPristine();
            };

            $scope.deleteLookupTable = function(index) {
                var lookupTable = $scope.lookupTables[index];
                var lookupTableName = lookupTable.name || "lookup table with no name";
                var modalInstance = confirmService.open(utilityService.createQuestionForDeleteConfirmationPopup(lookupTableName));

                modalInstance.result.then(function(confirmResult) {
                    if (confirmResult === 'ok') {
                        commandService.deleteLookupTable($state.params.questionnaireId, lookupTable.itemId).success(function() {
                            $scope.lookupTables.splice(index, 1);
                        });
                    }
                });
            };

            $scope.foldback = function () {
                $scope.isFolded = false;
                $rootScope.$broadcast("closeLookupTables", {});
            };

            $scope.$on('openLookupTables', function (scope, params) {
                $scope.unfold();
                if (!_.isUndefined(params) && !_.isUndefined(params.focusOn)) {
                    setTimeout(function () { utilityService.focus("focusLookupTable" + params.focusOn); }, 500);
                }
            });


            $scope.$on('closeLookupTablesRequested', function () {
                $scope.foldback();
            });

            $rootScope.$on('questionnaireLoaded', function () {
                $scope.loadLookupTables();
            });

            $scope.$on('verifing', function (scope, params) {
                for (var i = 0; i < $scope.lookupTables.length; i++) {
                    var lookupTable = $scope.lookupTables[i];
                    if (lookupTable.form.$dirty) {
                        $scope.saveLookupTable(lookupTable);
                    }
                }
            });
        });
