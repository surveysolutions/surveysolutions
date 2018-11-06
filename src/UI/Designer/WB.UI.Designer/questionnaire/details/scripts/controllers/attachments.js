angular.module('designerApp')
    .controller('AttachmentsCtrl',
        function ($rootScope, $scope, $state, $i18next, hotkeys, commandService, utilityService, confirmService, Upload, $uibModal, notificationService, moment) {
            'use strict';

            $scope.downloadLookupFileBaseUrl = '../../attachments';
            $scope.benchmarkDownloadSpeed = 20;
            $scope.isReadOnlyForUser = false;
            var recommendedMaxResolution = 1024;
            var KB = 1024;
            var MB = KB * KB;


            var hideAttachmentsPane = 'ctrl+shift+a';

            if (hotkeys.get(hideAttachmentsPane) !== false) {
                hotkeys.del(hideAttachmentsPane);
            }

            hotkeys.add(hideAttachmentsPane, $i18next.t('HotkeysHideAttachments'), function (event) {
                event.preventDefault();
                $scope.foldback();
            });

            $scope.attachments = [];
            $scope.totalSize = function () {
                return _.reduce($scope.attachments, function (sum, attachment) {
                    return sum + (attachment.content.size || 0);
                }, 0);
            };

            $scope.estimatedLoadingTime = function () {
                return Math.floor($scope.totalSize() / $scope.benchmarkDownloadSpeed);
            }

            $scope.isTotalSizeTooBig = function () {
                return $scope.totalSize() > 50 * MB;
            };

            $scope.isAttachmentSizeTooBig = function (attachment) {
                return attachment.content.size > 5 * MB;
            };

            $scope.isAttachmentResolutionTooBig = function (attachment) {
                return ((attachment.content.details.height || 0) > recommendedMaxResolution) || ((attachment.content.details.width || 0) > recommendedMaxResolution);
            };

            var dataBind = function (attachment, attachmentDto) {
                attachment.attachmentId = attachmentDto.attachmentId;
                attachment.name = attachmentDto.name;

                attachment.file = null;
                attachment.content = {};
                attachment.meta = {};
                attachment.content.details = {};

                if (!_.isUndefined(attachmentDto.content) && !_.isNull(attachmentDto.content)) {
                    attachment.content.size = attachmentDto.content.size;
                    attachment.content.type = attachmentDto.content.contentType;

                    if (!_.isUndefined(attachmentDto.content.details) && !_.isNull(attachmentDto.content.details)) {
                        attachment.content.details.height = attachmentDto.content.details.height;
                        attachment.content.details.width = attachmentDto.content.details.width;
                    }
                }

                if (!_.isUndefined(attachmentDto.meta) && !_.isNull(attachmentDto.meta)) {
                    attachment.meta.lastUpdated = moment.utc(attachmentDto.meta.lastUpdateDate).local();
                    attachment.meta.fileName = attachmentDto.meta.fileName;
                }
            };

            $scope.loadAttachments = function () {
                if ($scope.questionnaire === null)
                    return;

                $scope.isReadOnlyForUser = $scope.questionnaire.isReadOnlyForUser || false;

                if ($scope.questionnaire.attachments === null)
                    return;

                _.each($scope.questionnaire.attachments, function (attachmentDto) {
                    var attachment = {
                        checkpoint: {}
                    };
                    if (!_.any($scope.attachments, function (elem) {
                            return elem.attachmentId === attachmentDto.attachmentId;
                        })) {
                        dataBind(attachment, attachmentDto);
                        dataBind(attachment.checkpoint, attachmentDto);
                        $scope.attachments.push(attachment);
                    }
                });
            };

            $scope.formatBytes = function (bytes) {
                if (bytes === 0) return '0 Byte';
                var base = KB;
                var sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB', 'PB', 'EB', 'ZB', 'YB'];
                var degree = Math.min(Math.floor(Math.log(bytes) / Math.log(base)), sizes.length - 1);
                var decimalPlaces = Math.min(Math.max(degree - 1, 0), 2);
                return parseFloat((bytes / Math.pow(base, degree)).toFixed(decimalPlaces)) + ' ' + sizes[degree];
            }

            $scope.formatSeconds = function (seconds) {
                return moment.duration(seconds).humanize();
            }

            $scope.isFolded = false;

            $scope.unfold = function () {
                $scope.isFolded = true;
            };

            $scope.createAndUploadFile = function (file) {
                if (_.isNull(file) || _.isUndefined(file)) {
                    return;
                }

                if ($scope.isReadOnlyForUser) {
                    notificationService.notice($i18next.t('NoPermissions'));
                    return;
                }

                var attachment = {
                    attachmentId: utilityService.guid(),
                    checkpoint: {}
                };

                $scope.fileSelected(attachment, file, function () {
                    commandService.updateAttachment($state.params.questionnaireId, attachment.attachmentId, attachment).then(function () {
                        dataBind(attachment.checkpoint, attachment);
                        $scope.attachments.push(attachment);
                        setTimeout(function () {
                            utilityService.focus("focusAttachment" + attachment.attachmentId);
                        }, 500);
                    });
                });
            };

            $scope.fileSelected = function (attachment, file, callback) {
                if (_.isUndefined(file) || _.isNull(file)) {
                    return;
                }

                var fillFileMetaInfo = function () {
                    attachment.file = file;

                    attachment.content = {};
                    attachment.content.size = file.size;
                    attachment.content.type = file.type;

                    attachment.content.details = {};

                    attachment.meta = {};
                    attachment.meta.fileName = file.name;
                    attachment.meta.lastUpdated = moment();

                    if (attachment.meta.fileName) {
                        var maxAttachmentNameLength = 32;
                        var attachmentFileNameLength = attachment.meta.fileName.length;

                        attachment.name = attachment.meta.fileName.replace(/\.[^/.]+$/, "")
                            .substring(0, attachmentFileNameLength < maxAttachmentNameLength ?
                                attachmentFileNameLength :
                                maxAttachmentNameLength);
                    }
                    if (!_.isUndefined(attachment.form)) {
                        attachment.form.$setDirty();
                    }

                    if (!_.isUndefined(callback)) {
                        callback();
                    }
                }

                if (file.type === 'application/pdf') {
                    fillFileMetaInfo();
                }

                if (file.type.startsWith('video')) {
                    fillFileMetaInfo();
                }

                if (file.type.startsWith('audio')) {
                    fillFileMetaInfo();
                }

                if (file.type.startsWith('image')) {
                    Upload.imageDimensions(file).then(function (dimensions) {
                            fillFileMetaInfo();
                            attachment.content.details.height = dimensions.height;
                            attachment.content.details.width = dimensions.width;

                        })
                        .catch(function () {
                            notificationService.error($i18next.t('NotSupportedAttachment'));
                        });
                }
            }

            $scope.saveAttachment = function (attachment) {

                attachment.oldAttachmentId = attachment.attachmentId;
                // each time we modify attachment, we give it new id to make revert to version works correctly
                var newAttachmentId = utilityService.guid();

                commandService.updateAttachment($state.params.questionnaireId, newAttachmentId, attachment).then(function () {
                    // only if request didn't fail, we update id of attachment with new value to not loose connection with content id.
                    attachment.attachmentId = newAttachmentId;
                    dataBind(attachment.checkpoint, attachment);
                    attachment.form.$setPristine();
                });
            };


            $scope.$on('verifing', function (scope, params) {
                for (var i = 0; i < $scope.attachments.length; i++) {
                    var attachment = $scope.attachments[i];
                    if (attachment.form.$dirty) {
                        $scope.saveAttachment(attachment);
                    }
                }
            });

            $scope.cancel = function (attachment) {
                dataBind(attachment, attachment.checkpoint);
                attachment.form.$setPristine();
            };

            $scope.deleteAttachment = function (index) {
                var attachment = $scope.attachments[index];
                var attachmentName = attachment.name || $i18next.t('SideBarAttachmentNoName');
                var modalInstance = confirmService.open(utilityService.createQuestionForDeleteConfirmationPopup(attachmentName));

                modalInstance.result.then(function (confirmResult) {
                    if (confirmResult === 'ok') {
                        commandService.deleteAttachment($state.params.questionnaireId, attachment.attachmentId).then(function () {
                            $scope.attachments.splice(index, 1);
                        });
                    }
                });
            };

            $scope.previewAttachment = function (attachment) {
                var baseURL = $scope.downloadLookupFileBaseUrl;
                var modalInstance = $uibModal.open({
                    templateUrl: 'views/attachment-preview.html',
                    controller: function ($scope, attachment) {
                        $scope.attachment = attachment;

                        $scope.cancel = function () {
                            modalInstance.dismiss();
                        };

                        $scope.attachmentUrl = function () {
                            return baseURL + "/thumbnail/" + $scope.attachment.attachmentId + '/568';
                        }

                    },
                    windowClass: 'attachment-preview-window',
                    resolve: {
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
                $scope.unfold();
                if (!_.isUndefined(params) && !_.isUndefined(params.focusOn)) {
                    setTimeout(function () {
                        utilityService.focus("focusAttachment" + params.focusOn);
                    }, 500);
                }
            });


            $scope.$on('closeAttachmentsRequested', function () {
                $scope.foldback();
            });

            $rootScope.$on('questionnaireLoaded', function () {
                $scope.loadAttachments();
            });
        });
