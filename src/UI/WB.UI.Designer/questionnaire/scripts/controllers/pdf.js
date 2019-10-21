angular
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

        $scope.viewModel.translations = $scope.questionnaire.translations;
        $scope.viewModel.translations.splice(0, 0, { id: null, name: $i18next.t("Translation_Original") });
        $scope.viewModel.selectedTranslation = $scope.viewModel.translations[0];

        var generateTimerId = null;

        $scope.generate = function () {
            updateExportPdfStatus();
        };

        var updateExportPdfStatus = function () {
            var request = pdfService.updateExportPdfStatus($scope.questionnaire.questionnaireId,
                $scope.viewModel.selectedTranslation.id);

            request.then(function(result) {
                var data = result.data;

                if (data.message !== null)
                    $scope.viewModel.generateStatusMessage = data.message;
                else
                    $scope.viewModel.generateStatusMessage =
                        "Unexpected server response.\r\nPlease contact support@mysurvey.solutions if problem persists.";

                $scope.viewModel.canDownload = data.readyForDownload;
                $scope.viewModel.canRetryGenerate = data.canRetry;

                if (data.message === null) return;

                if (!$scope.viewModel.canDownload) {
                    generateTimerId = setTimeout(function() {
                            updateExportPdfStatus($scope.viewModel.selectedTranslation);
                        },
                        1500);
                }
                else $scope.isGenerating = false;
            });
        };

        $scope.selectTranslation = function(translation) {
            $scope.selectedTranslation = translation;
        };

        $scope.retryGenerate = function() {

        };
        $scope.download = function () {
            $scope.cancel();
            window.location = '../../pdf/download/' + $scope.questionnaire.questionnaireId +
                '?translation=' + $scope.viewModel.selectedTranslation.id;
        };

        $scope.cancel = function () {
            clearTimeout(generateTimerId);
            $uibModalInstance.close();
        };
});
