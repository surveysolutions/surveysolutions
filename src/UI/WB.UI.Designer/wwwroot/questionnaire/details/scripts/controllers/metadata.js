angular.module('designerApp')
    .controller('MetadataCtrl',
    function ($rootScope, $scope, $state, $i18next, hotkeys, commandService, utilityService, confirmService) {
            'use strict';

            var hideMetadataPanel = 'ctrl+i';

            if (hotkeys.get(hideMetadataPanel) !== false) {
                hotkeys.del(hideMetadataPanel);
            }

            hotkeys.add(hideMetadataPanel, $i18next.t('HotkeysCloseMetadata'), function (event) {
                event.preventDefault();
                $scope.foldback();
            });

            $scope.metadata = {};

            var dataBind = function (metadata, metadataDto) {
                metadata.title = metadataDto.title;
                metadata.subTitle = metadataDto.subTitle;
                metadata.studyType = metadataDto.studyType;
                metadata.version = metadataDto.version;
                metadata.versionNotes = metadataDto.versionNotes;
                metadata.kindOfData = metadataDto.kindOfData;
                metadata.country = metadataDto.country;
                metadata.year = metadataDto.year;
                metadata.language = metadataDto.language;
                metadata.coverage = metadataDto.coverage;
                metadata.universe = metadataDto.universe;
                metadata.unitOfAnalysis = metadataDto.unitOfAnalysis;
                metadata.primaryInvestigator = metadataDto.primaryInvestigator;
                metadata.funding = metadataDto.funding;
                metadata.consultant = metadataDto.consultant;
                metadata.modeOfDataCollection = metadataDto.modeOfDataCollection;
                metadata.notes = metadataDto.notes;
                metadata.keywords = metadataDto.keywords;
                metadata.agreeToMakeThisQuestionnairePublic = metadataDto.agreeToMakeThisQuestionnairePublic;
            };

            $scope.loadMetadata = function () {
                if ($scope.questionnaire === null || $scope.questionnaire.metadata === null)
                    return;

                dataBind($scope.metadata, $scope.questionnaire.metadata);
                $scope.metadata.checkpoint = { };
                dataBind($scope.metadata.checkpoint, $scope.questionnaire.metadata);
            };

            $scope.isFolded = false;

            $scope.unfold = function () {
                $scope.isFolded = true;
            };

            $scope.saveMetadata = function () {
                commandService.updateMetadata($state.params.questionnaireId, $scope.metadata).then(function () {
                    dataBind($scope.metadata.checkpoint, $scope.metadata);
                    $scope.metadata.form.$setPristine();
                    $scope.questionnaire.title = $scope.metadata.title;
                });
            };

            $scope.cancelMetadata = function () {
                dataBind($scope.metadata, $scope.metadata.checkpoint);
                $scope.metadata.form.$setPristine();
            };

            $scope.foldback = function () {
                $scope.isFolded = false;
                $rootScope.$broadcast("closeMetadata", {});
            };

            $scope.$on('openMetadata', function (scope, params) {
                $scope.unfold();
            });

            $scope.$on('closeMetadataRequested', function () {
                $scope.foldback();
            });

            $rootScope.$on('questionnaireLoaded', function () {
                $scope.loadMetadata();
            });

            $scope.$on('verifing', function () {
                if ($scope.metadata.form.$dirty) {
                    $scope.saveMetadata();
                }
            });
        });
