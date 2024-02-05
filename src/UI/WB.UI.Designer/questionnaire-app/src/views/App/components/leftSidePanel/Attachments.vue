<template>
    <div style="color: brown; font-size: large;">Under construction</div>
    <div class="attachments">
        <perfect-scrollbar class="scroller">
            <div class="panel-header clearfix">
                <div class="title pull-left">
                    <h3>
                        {{ $t('QuestionnaireEditor.SideBarAttachmentsCounter', {
                            count: attachments.length,
                            bytes: formatBytes(totalSize())
                        }) }}
                    </h3>
                    <p class="estimated-download-time">
                        {{ $t('QuestionnaireEditor.SideBarAttachmentsEstimate', {
                            timeString: formatSeconds(estimatedLoadingTime()),
                            downloadSpeed: benchmarkDownloadSpeed
                        }) }}
                    </p>
                </div>
                <!-- <button class="btn btn-default btn-lg pull-left" :class="{ 'btn-primary': !isReadOnlyForUser }" ngf-select
                    ngf-change="createAndUploadFile($file);$event.stopPropagation()"
                    ngf-accept="'.pdf,image/*,video/*,audio/*'" ngf-max-size="100MB" type="file"
                    ngf-select-disabled="isReadOnlyForUser" ngf-drop-disabled="isReadOnlyForUser"
                    :disabled="isReadOnlyForUser">
                    {{ $t('QuestionnaireEditor.SideBarAttachmentsUpload') }}
                </button> -->

                <input type="button" :value="$t('QuestionnaireEditor.SideBarAttachmentsUpload')"
                    @click.stop="openFileDialog()" value="Upload new attachment" class="btn btn-default btn-lg pull-left"
                    :class="{ 'btn-primary': !isReadOnlyForUser }" ngf-select :disabled="isReadOnlyForUser" capture />

                <file-upload ref="upload" v-if="!isReadOnlyForUser" :input-id="'tfunew'" v-model="file"
                    :size="100 * 1024 * 1024" :drop="false" :drop-directory="false" @input-file="createAndUploadFile"
                    accept=".pdf,image/*,video/*,audio/*">
                </file-upload>
            </div>
            <div class="empty-list" v-if="attachments.length == 0">
                <p> {{ $t('QuestionnaireEditor.SideBarAttachmentsEmptyLine1') }} </p>
                <p>
                    <span>{{ $t('QuestionnaireEditor.SideBarAttachmentsEmptyLine2') }}</span>
                    <a href="https://support.mysurvey.solutions/questionnaire-designer/limits/multimedia-reference"
                        target="_blank">
                        {{ $t('QuestionnaireEditor.ClickHere') }}
                    </a>
                </p>
                <p v-html="emptyAttachmentsDescription" />
            </div>
            <form role="form" name="attachmentsForm" novalidate>
                <div class="attachment-list">
                    <div name="attachment.form" v-for="attachment in attachments">
                        <div class="attachments-panel-item" :class="{ 'has-error': isNameValid(attachment.name) }"
                            ngf-drop="" ngf-max-size="100MB" ngf-change="fileSelected(attachment, $file)"
                            ngf-drag-over-class="{accept:'dragover', reject:'dragover-err'}">
                            <a href="javascript:void(0);" @click="deleteAttachment(attachment)"
                                :disabled="isReadOnlyForUser" v-if="!isReadOnlyForUser" class="btn delete-btn"
                                tabindex="-1"></a>
                            <div class="attachment">
                                <div class="attachment-preview">
                                    <div class="attachment-preview-cover clearfix">
                                        <img class="pull-right" @click="previewAttachment(attachment)"
                                            ngf-size="{width: 156, height: 140}"
                                            :src='downloadLookupFileBaseUrl + "/" + questionnaire.questionnaireId + "/thumbnail/" + attachment.attachmentId'>
                                    </div>
                                </div>
                                <div class="attachment-content">
                                    <input focus-on-out="focusAttachment{{attachment.attachmentId}}" required=""
                                        maxlength="32" spellcheck="false" v-model="attachment.name" name="name"
                                        class="form-control table-name" type="text"
                                        :placeholder="$t('QuestionnaireEditor.SideBarAttachmentName')" />
                                    <div class="divider"></div>
                                    <div class="drop-box">
                                        {{ $t('QuestionnaireEditor.SideBarLookupTableDropFile') }}
                                    </div>
                                    <div class="attachment-meta">
                                        <p>
                                            <span class="file-name">{{ attachment.meta.fileName }}</span><br>
                                            <span :class="{ 'error': isAttachmentResolutionTooBig(attachment) }"
                                                v-if="attachment.content.details.width">
                                                {{ $t('QuestionnaireEditor.SideBarAttachmentDetailsResolution', {
                                                    width: attachment.content.details.width,
                                                    height: attachment.content.details.height
                                                }) }}
                                            </span>
                                            {{ attachment.content.details.width ? ',&nbsp;' : '' }}
                                            <span>
                                                {{ $t('QuestionnaireEditor.SideBarAttachmentDetailsFormat', {
                                                    type: attachment.content.type
                                                }) }}
                                            </span>,
                                            <span :class="{ 'error': isAttachmentSizeTooBig(attachment) }">
                                                {{ $t('QuestionnaireEditor.SideBarAttachmentDetailsSize', {
                                                    size: formatBytes(attachment.content.size)
                                                }) }}</span>
                                        </p>
                                        <p>
                                            {{ $t('QuestionnaireEditor.SideBarAttachmentUploaded', {
                                                lastUpdate: attachment.meta.lastUpdated
                                            }) }}
                                        </p>
                                    </div>
                                    <div class="actions clearfix" :class="{ 'dirty': isDirty(attachment) }">
                                        <div v-if="isDirty(attachment)" class="pull-left">
                                            <button type="button"
                                                :disabled="questionnaire.isReadOnlyForUser || attachment.form.$invalid"
                                                class="btn lighter-hover" @click="saveAttachment(attachment)">{{
                                                    $t('QuestionnaireEditor.Save') }}</button>
                                            <button type="button" class="btn lighter-hover" @click="cancel(attachment)">{{
                                                $t('QuestionnaireEditor.Cancel') }}</button>
                                        </div>
                                        <div class="permanent-actions pull-right clearfix">
                                            <!-- <button :disabled="isReadOnlyForUser" class="btn btn-default pull-right"
                                                ngf-select="" ngf-accept="'.pdf,image/*,video/*,audio/*'"
                                                ngf-max-size="100MB"
                                                ngf-change="fileSelected(attachment, $file);$event.stopPropagation()"
                                                type="file">
                                                <span>{{ $t('QuestionnaireEditor.Update') }}</span>
                                            </button> -->

                                            <input type="button" :value="$t('QuestionnaireEditor.SideBarAttachmentsUpload')"
                                                @click.stop="openFileDialog()" value="Upload new attachment"
                                                class="btn btn-default btn-lg pull-left"
                                                :class="{ 'btn-primary': !isReadOnlyForUser }" ngf-select
                                                :disabled="isReadOnlyForUser" capture />

                                            <file-upload ref="upload" v-if="!isReadOnlyForUser" :input-id="'tfunew'"
                                                v-model="file" :size="100 * 1024 * 1024" :drop="false"
                                                :drop-directory="false" @input-file="createAndUploadFile"
                                                accept=".pdf,image/*,video/*,audio/*">
                                            </file-upload>


                                            <a :href="downloadLookupFileBaseUrl + '/' + questionnaire.questionnaireId + '/' + attachment.attachmentId"
                                                class="btn btn-default pull-right" target="_blank"
                                                rel="noopener noreferrer">{{ $t('QuestionnaireEditor.Download') }}</a>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </form>
        </perfect-scrollbar>
    </div>
</template>
  
<script>

import _ from 'lodash';
import moment from 'moment';
import { newGuid } from '../../../../helpers/guid';
import { createQuestionForDeleteConfirmationPopup } from '../../../../services/utilityService'
import { useQuestionnaireStore } from '../../../../stores/questionnaire';
import { deleteAttachment } from '../../../../services/attachmentsService';
import { notice } from '../../../../services/notificationService';


export default {
    name: 'Attachments',
    inject: ['isReadOnlyForUser'],
    props: {},
    data() {
        return {
            benchmarkDownloadSpeed: 20,
            downloadLookupFileBaseUrl: '/attachments',
        }
    },
    setup() {
        const questionnaireStore = useQuestionnaireStore();

        return {
            questionnaireStore,
        };
    },
    computed: {
        questionnaire() {
            return this.questionnaireStore.getInfo;
        },

        attachments() {
            return this.questionnaireStore.getEdittingAttachments;
        },
    },
    methods: {
        openFileDialog() {
            const fu = this.$refs.upload
            fu.$el.querySelector("#" + fu.inputId).click()
        },
        formatBytes(bytes) {
            if (bytes === 0) return '0 Byte';

            var KB = 1024;
            var MB = KB * KB;

            var base = KB;
            var sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB', 'PB', 'EB', 'ZB', 'YB'];
            var degree = Math.min(Math.floor(Math.log(bytes) / Math.log(base)), sizes.length - 1);
            var decimalPlaces = Math.min(Math.max(degree - 1, 0), 2);
            return parseFloat((bytes / Math.pow(base, degree)).toFixed(decimalPlaces)) + ' ' + sizes[degree];
        },
        formatSeconds(seconds) {
            return moment.duration(seconds).humanize();
        },
        estimatedLoadingTime() {
            return Math.floor(this.totalSize() / this.benchmarkDownloadSpeed);
        },
        totalSize() {
            return _.reduce(this.attachments, function (sum, attachment) {
                return sum + (attachment.content.size || 0);
            }, 0);
        },
        saveAttachment(attachment) {
            // if (attachment.form.$invalid) return;
            // this.$emit('save-attachment', attachment);
        },
        cancel(attachment) {
            // this.$emit('cancel-attachment', attachment);
        },
        isNameValid(name) {
            return name && name.length < 32;
        },
        isAttachmentResolutionTooBig(attachment) {
            return false;
            //return attachment.content.details.width > 1920 || attachment.content.details.height > 1080;
        },
        isAttachmentSizeTooBig(attachment) {
            return false;
            //return attachment.content.size > 100 * MB;
        },
        previewAttachment(attachment) {
            // this.$emit('preview-attachment', attachment);
        },
        deleteAttachment(attachment) {
            var attachmentName = attachment.name || this.$t('QuestionnaireEditor.SideBarAttachmentName');

            const questionnaireId = this.questionnaire.questionnaireId;
            const attachmentId = attachment.attachmentId;
            var confirmParams = createQuestionForDeleteConfirmationPopup(attachmentName)

            confirmParams.callback = confirm => {
                if (confirm) {
                    deleteAttachment(questionnaireId, attachmentId)
                }
            };

            this.$confirm(confirmParams);
        },
        fileSelected(attachment, file) {
            // this.$emit('file-selected', attachment, file);
        },
        isDirty(attachment) {
            return false;
            //return some(this.attachments, function (attachment) {
            //    return attachment.form.$dirty;
            //});
        },
        async createAndUploadFile(file) {
            if (_.isNull(file) || _.isUndefined(file)) {
                return;
            }

            if (this.isReadOnlyForUser) {
                notice(this.$t('QuestionnaireEditor.NoPermissions'));
                return;
            }
            var attachment = {
                attachmentId: newGuid(),
                checkpoint: {}
            };

            // $scope.fileSelected(attachment, file, function () {
            //     commandService.updateAttachment($state.params.questionnaireId, attachment.attachmentId, attachment)
            //         .then(function (result) {
            //             if (result && result.status == 200) {
            //                 dataBind(attachment.checkpoint, attachment);
            //                 attachment.file = null;
            //                 $scope.attachments.push(attachment);
            //                 setTimeout(function () {
            //                     utilityService.focus("focusAttachment" + attachment.attachmentId);
            //                 }, 500);
            //             }
            //         });
            // });



            // var fillFileMetaInfo = function () {
            //     attachment.file = file;

            //     attachment.content = {};
            //     attachment.content.size = file.size;
            //     attachment.content.type = file.type;

            //     attachment.content.details = {};

            //     attachment.meta = {};
            //     attachment.meta.fileName = file.name;
            //     attachment.meta.lastUpdated = moment();

            //     if (attachment.meta.fileName) {
            //         var maxAttachmentNameLength = 32;
            //         var attachmentFileNameLength = attachment.meta.fileName.length;

            //         attachment.name = attachment.meta.fileName.replace(/\.[^/.]+$/, "")
            //             .replace(" ", "_")
            //             .substring(0, attachmentFileNameLength < maxAttachmentNameLength ?
            //                 attachmentFileNameLength :
            //                 maxAttachmentNameLength);
            //     }
            //     if (!_.isUndefined(attachment.form)) {
            //         attachment.form.$setDirty();
            //     }

            //     if (!_.isUndefined(callback)) {
            //         callback();
            //     }
            // }

            // if (file.type === 'application/pdf') {
            //     fillFileMetaInfo();
            // }

            // if (file.type.startsWith('video')) {
            //     fillFileMetaInfo();
            // }

            // if (file.type.startsWith('audio')) {
            //     fillFileMetaInfo();
            // }

            // if (file.type.startsWith('image')) {
            //     Upload.imageDimensions(file)
            //         .then(function (dimensions) {
            //             if (((dimensions.height || 0) > allowedMaxResolution)
            //                 || ((dimensions.width || 0) > allowedMaxResolution)) {
            //                 notificationService.error($i18next.t('AttachmentDimensionsAreTooBig'));
            //                 return;
            //             }
            //             fillFileMetaInfo();
            //             attachment.content.details.height = dimensions.height;
            //             attachment.content.details.width = dimensions.width;
            //         })
            //         .catch(function () {
            //             notificationService.error($i18next.t('NotSupportedAttachment'));
            //         });
            // }



            // let translation = {};
            // translation.file = file.file;

            // translation.content = {};
            // translation.content.size = file.size;
            // translation.content.type = file.type;

            // translation.meta = {};
            // translation.meta.fileName = file.name;
            // translation.meta.lastUpdated = moment();

            // const suspectedTranslations = translation.meta.fileName.match(/[^[\]]+(?=])/g);

            // if (suspectedTranslations && suspectedTranslations.length > 0)
            //     translation.name = suspectedTranslations[0];
            // else
            //     translation.name = translation.meta.fileName.replace(/\.[^/.]+$/, "");

            // const maxNameLength = 32;
            // const fileNameLength = translation.name.length;
            // translation.name = translation.name.substring(0, fileNameLength < maxNameLength ? fileNameLength : maxNameLength);
            // translation.oldTranslationId = null;
            // translation.translationId = newGuid();

            // const response = await this.questionnaireStore.addTranslation(translation);

            // if (translation.file) notice(response);
            // translation.file = null;
            // this.file = [];

            //setTimeout(function () { utilityService.focus("focusTranslation" + translation.translationId); }, 500);
        },
    }
}
</script>
  