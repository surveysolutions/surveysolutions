﻿angular
    .module('designerApp')
    .controller('pdfCtrl', function(
        $scope,
        $log,
        $i18next,
        $uibModalInstance,
        questionnaire,
        currentUser,
        pdfService
    ) {
        'use strict';

        $scope.currentUser = currentUser;
        $scope.questionnaire = questionnaire;

        $scope.viewModel = {
            translations: [],
            canDownload: false,
            canRetryGenerate: false,
            isGenerating: false,
            selectedTranslation: null,
            generateStatusMessage: ''
        };

        $scope.viewModel.translations = _.map($scope.questionnaire.translations, _.clone);
        $scope.viewModel.translations.splice(0, 0, { translationId: null, name: $i18next.t("Translation_Original") });
        $scope.viewModel.selectedTranslation = $scope.viewModel.translations[0];

        var generateTimerId = null;

        $scope.selectTranslation = function(translation) {
            $scope.viewModel.selectedTranslation = translation;
        };

        $scope.cancel = function () {
            clearTimeout(generateTimerId);
            $uibModalInstance.close();
        };

        $scope.retryGenerate = function () {
            var translationId = $scope.viewModel.selectedTranslation.translationId;

            pdfService.retryExportPdf($scope.questionnaire.questionnaireId,
                translationId);

            updateExportPdfStatus(translationId);
        };

        $scope.generate = function () {
            $scope.viewModel.generateStatusMessage = $i18next.t("Initializing") + '...';
            $scope.viewModel.isGenerating = true;

            updateExportPdfStatus($scope.viewModel.selectedTranslation.translationId);
        };

        var updateExportPdfStatus = function (translationId) {

            var request = pdfService.updateExportPdfStatus($scope.questionnaire.questionnaireId,
                translationId);

            request.then(function(result) {
                onExportPdfStatusReceived(result.data, translationId);
            });
        };

        var onExportPdfStatusReceived = function(data, translationId) {

            if (data.message !== null)
                $scope.viewModel.generateStatusMessage = data.message;
            else
                $scope.viewModel.generateStatusMessage =
                    "Unexpected server response.\r\nPlease contact support@mysurvey.solutions if problem persists.";

            $scope.viewModel.canDownload = data.readyForDownload;
            $scope.viewModel.canRetryGenerate = data.canRetry;

            if (!$scope.viewModel.canDownload) {
                if ($scope.viewModel.canRetryGenerate) {
                    clearTimeout(generateTimerId);
                    $scope.viewModel.isGenerating = false;
                } else {
                    generateTimerId = setTimeout(function() {
                            updateExportPdfStatus(translationId);
                        },
                        1500);
                }
            }
            else {
                $scope.cancel();
                window.location = '../../pdf/download/' + $scope.questionnaire.questionnaireId +
                    '?translation=' + translationId;
            }
        };
});
