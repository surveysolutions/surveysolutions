angular.module('designerApp')
    .controller('AttachmentsCtrl',
        function ($rootScope, $scope, $state, hotkeys, commandService, utilityService, confirmService, Upload, $modal) {
            'use strict';

            $scope.downloadLookupFileBaseUrl = '../../attachment/';
            var hideAttachmentsPane = 'ctrl+l';

            if (hotkeys.get(hideAttachmentsPane) !== false) {
                hotkeys.del(hideAttachmentsPane);
            }

            hotkeys.add(hideAttachmentsPane, 'Close attachments panel', function (event) {
                event.preventDefault();
                $scope.foldback();
            });

            $scope.attachments = [];
            $scope.totalSize = function() {
                return _.reduce($scope.attachments, function (sum, attachment) {
                    return sum + (attachment.sizeInBytes || 0);
                }, 0);
            };

            var dataBind = function (attachment, attachmentDto) {
                attachment.initialAttachment = angular.copy(attachmentDto);

                attachment.itemId = attachmentDto.itemId;
                attachment.type = attachmentDto.type;
                attachment.name = attachmentDto.name;
                attachment.fileName = attachmentDto.fileName;

                if (!_.isUndefined(attachmentDto.meta) && !_.isNull(attachmentDto.meta)) {
                    attachment.format = attachmentDto.meta.format;
                    attachment.height = attachmentDto.meta.height;
                    attachment.width = attachmentDto.meta.width;
                }

                attachment.lastUpdated = attachmentDto.lastUpdated;
                attachment.sizeInBytes = attachmentDto.sizeInBytes;
                
                attachment.file = null;
                attachment.hasUploadedFile = !_.isEmpty(attachmentDto.fileName);
            };

            $scope.loadAttachments = function () {
                if ($scope.questionnaire === null || $scope.questionnaire.attachments === null)
                    return;

                _.each($scope.questionnaire.attachments, function (attachmentDto) {
                    var attachment = {};
                    if (!_.any($scope.attachments, 'itemId', attachmentDto.itemId))
                    {
                        dataBind(attachment, attachmentDto);
                        $scope.attachments.push(attachment);
                    }
                });
            };

            $scope.formatBytes = function (bytes)
            {
                if (bytes === 0) return '0 Byte';
                var base = 1024;
                var sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB', 'PB', 'EB', 'ZB', 'YB'];
                var degree = Math.min(Math.floor(Math.log(bytes) / Math.log(base)), sizes.length - 1);
                var decimalPlaces = Math.min(Math.max(degree - 1, 0), 2);
                return parseFloat((bytes / Math.pow(base, degree)).toFixed(decimalPlaces)) + ' ' + sizes[degree];
            }

            $scope.isFolded = false;

            $scope.unfold = function () {
                $scope.isFolded = true;
            };

            $scope.addNewAttachment = function () {
                var newId = utilityService.guid();

                var newAttachments = {
                    itemId: newId
                };

                commandService.addAttachment($state.params.questionnaireId, newAttachments).success(function () {
                    var attachments = {};
                    dataBind(attachments, newAttachments);
                    $scope.attachments.push(attachments);
                });
            };

            $scope.fileSelected = function (attachment, file, attachmentForm) {
                if (_.isUndefined(file) || _.isNull(file)) {
                    return;
                }
                attachment.file = file;
                attachment.fileName = attachment.file.name;
                attachment.format = file.type;
                attachment.sizeInBytes = file.size;
                attachment.type = "Image";
                
                Upload.imageDimensions(file).then(function(dimensions) {
                    attachment.height = dimensions.height;
                    attachment.width = dimensions.width;
                });
                attachmentForm.$setDirty();
            }

            $scope.saveAttachment = function (attachment, form) {
                commandService.updateAttachment($state.params.questionnaireId, attachment).success(function() {
                    attachment.initialAttachment = angular.copy(attachment);
                    attachment.hasUploadedFile = !_.isEmpty(attachment.fileName);
                    form.$setPristine();
                });
            };

            $scope.cancel = function(attachment, form) {
                var temp = angular.copy(attachment.initialAttachment);
                dataBind(attachment, temp);
                form.$setPristine();
            };

            $scope.deleteAttachment = function(index) {
                var attachment = $scope.attachments[index];
                var attachmentName = attachment.name || "attachment with no name";
                var modalInstance = confirmService.open(utilityService.createQuestionForDeleteConfirmationPopup(attachmentName));
                 
                modalInstance.result.then(function(confirmResult) {
                    if (confirmResult === 'ok') {
                        commandService.deleteAttachment($state.params.questionnaireId, attachment.itemId).success(function() {
                            $scope.attachments.splice(index, 1);
                        });
                    }
                });
            };

            $scope.previewAttachment = function (attachment) {
                var baseURL = $scope.downloadLookupFileBaseUrl;
                var modalInstance = $modal.open({
                    templateUrl: 'views/attachment-preview.html',
                    controller: function ($scope, attachment) {
                        $scope.attachment = attachment;

                        $scope.cancel = function () {
                            modalInstance.dismiss();
                        };

                        $scope.attachmentUrl = function() {
                            return baseURL + $scope.attachment.itemId;
                        }
                       
                    },
                    windowClass: 'attachment-preview-window',
                    resolve:
                    {
                        attachment: function () {
                            return attachment;
                        }
                    }
                });
            }

            $scope.foldback = function () {
                $scope.isFolded = false;
                $rootScope.$broadcast("closeAttachments", {});
            };

            $scope.$on('openAttachments', function (scope, params) {

                $rootScope.$broadcast("closeChaptersListRequested", {});
                $rootScope.$broadcast("closeMacrosListRequested", {});

                $scope.unfold();
                if (!_.isUndefined(params) && !_.isUndefined(params.focusOn)) {
                    setTimeout(function () { utilityService.focus("focusAttachment" + params.focusOn); }, 500);
                }
            });


            $scope.$on('closeAttachmentsRequested', function () {
                $scope.foldback();
            });

            $rootScope.$on('questionnaireLoaded', function () {
                $scope.loadAttachments();
            });
        });
