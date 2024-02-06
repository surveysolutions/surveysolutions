<template>
    <div name="attachment.form">
        <div class="attachments-panel-item" :class="{ 'has-error': isNameValid(attachment.name) }">
            <a href="javascript:void(0);" @click="deleteAttachment(attachment)"
                :disabled="isReadOnlyForUser ? 'disabled' : null" v-if="!isReadOnlyForUser" class="btn delete-btn"
                tabindex="-1"></a>
            <div class="attachment">
                <div class="attachment-preview">
                    <div class="attachment-preview-cover clearfix">
                        <img class="pull-right" @click="previewAttachment(attachment)" ngf-size="{width: 156, height: 140}"
                            :src='downloadBaseUrl + "/" + questionnaireId + "/thumbnail/" + attachment.attachmentId'>
                    </div>
                </div>
                <div class="attachment-content">
                    <input focus-on-out="focusAttachment{{attachment.attachmentId}}" required="" maxlength="32"
                        spellcheck="false" v-model="attachment.name" name="name" class="form-control table-name" type="text"
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
                                    type: attachment.content.contentType || attachment.content.type
                                }) }}
                            </span>,
                            <span :class="{ 'error': isAttachmentSizeTooBig(attachment) }">
                                {{ $t('QuestionnaireEditor.SideBarAttachmentDetailsSize', {
                                    size: formatBytes(attachment.content.size)
                                }) }}</span>
                        </p>
                        <p>
                            {{ $t('QuestionnaireEditor.SideBarAttachmentUploaded', {
                                lastUpdate: attachment.meta.lastUpdateDate ||
                                    toLocalDateTime(attachment.meta.lastUpdateDate)
                            }) }}
                        </p>
                    </div>
                    <div class="actions clearfix" :class="{ 'dirty': isDirty }">
                        <div v-if="isDirty" class="pull-left">
                            <button type="button" :disabled="isReadOnlyForUser || !isInvalid" class="btn lighter-hover"
                                @click="saveAttachment(attachment)">{{
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
                            <!-- !!! -->
                            <button type="button" :value="$t('QuestionnaireEditor.SideBarAttachmentsUpload')"
                                @click.stop="openFileDialog()" value="Upload new attachment"
                                class="btn btn-default pull-right" ngf-select :disabled="isReadOnlyForUser" capture>
                                <span>{{ $t('QuestionnaireEditor.Update') }}</span>
                            </button>

                            <file-upload ref="upload" v-if="!isReadOnlyForUser" :input-id="'tfunew'" v-model="file"
                                :size="100 * 1024 * 1024" :drop="false" :drop-directory="false"
                                @input-file="updateFile(attachment, $file)" accept=".pdf,image/*,video/*,audio/*">
                            </file-upload>


                            <a :href="downloadLookupFileBaseUrl + '/' + questionnaire.questionnaireId + '/' + attachment.attachmentId"
                                class="btn btn-default pull-right" target="_blank" rel="noopener noreferrer">{{
                                    $t('QuestionnaireEditor.Download') }}</a>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<script>
import _ from 'lodash'
import moment from 'moment';
import { newGuid } from '../../../../helpers/guid';
import { toLocalDateTime, createQuestionForDeleteConfirmationPopup, formatBytes } from '../../../../services/utilityService'
import { deleteAttachment, updateAttachment } from '../../../../services/attachmentsService';

export default {
    name: 'AttachmentItem',
    props: {
        attachment: { type: Object, required: true },
        questionnaireId: { type: String, required: true },
    },
    inject: ['questionnaire', 'isReadOnlyForUser'],
    data() {
        return {
            downloadBaseUrl: '/attachments',
            initialName: null,
            file: [],
        }
    },
    computed: {
        hasUploadedFile() {
            return !_.isEmpty(this.attachment.fileName)
        },
        isDirty() {
            return this.attachment.name != this.initialName || this.file.length > 0;
        },
        isInvalid() {
            return true;
            //return this.isNameValid(this.attachment.name);
        },
    },
    beforeMount() {
        this.initialName = this.attachment.name;
    },
    methods: {
        formatBytes(bytes) {
            return formatBytes(bytes);
        },
        previewAttachment(attachment) {

            var srcImage = attachment.file || (this.downloadBaseUrl + "/" + this.questionnaireId + "/thumbnail/" + attachment.attachmentId + '/568');
            var confirmParams = {
                noControls: true,
                header: this.$t('QuestionnaireEditor.AttachmentPreview'),
                title: `<img class="attachment-preview-img" size="{width: 568, height: 568}" src="${srcImage}">`,
            }

            this.$confirm(confirmParams);
        },
        isAttachmentSizeTooBig(attachment) {
            return attachment.content.size > 5 * 1024 * 1824;
        },
        isAttachmentResolutionTooBig(attachment) {
            var recommendedMaxResolution = 1024;
            return ((attachment.content.details.height || 0) > recommendedMaxResolution) || ((attachment.content.details.width || 0) > recommendedMaxResolution);
        },
        isNameValid(name) {
            return true;
            //return name && name.length < 32;
        },
        //TODO move to reuse
        async updateFile(attachment, file, callback) {
            if (_.isNull(file) || _.isUndefined(file)) {
                return;
            }

            if (this.isReadOnlyForUser) {
                notice(this.$t('QuestionnaireEditor.NoPermissions'));
                return;
            }

            attachment.file = file.file,
                attachment.content = {
                    size: file.size,
                    type: file.type,
                    details: {}
                };
            attachment.meta = {
                lastUpdated: moment(),
                fileName: file.name
            };

            if (attachment.meta.fileName) {
                var maxAttachmentNameLength = 32;
                var attachmentFileNameLength = attachment.meta.fileName.length;

                attachment.name = attachment.meta.fileName.replace(/\.[^/.]+$/, "")
                    .replace(" ", "_")
                    .substring(0, attachmentFileNameLength < maxAttachmentNameLength ?
                        attachmentFileNameLength :
                        maxAttachmentNameLength);
            }
            self = this;

            if (file.type.startsWith('image')) {
                var reader = new FileReader();
                reader.onload = function (e) {
                    var image = new Image();
                    image.onload = async function () {
                        let width = image.width;
                        let height = image.height;
                        let w = image.naturalWidth;
                        let h = image.naturalHeight;

                        attachment.content.details.height = height;
                        attachment.content.details.width = width;

                        if (((attachment.content.details.height || 0) > self.allowedMaxResolution)
                            || ((attachment.content.details.width || 0) > self.allowedMaxResolution)) {
                            notice(self.$t('QuestionnaireEditor.AttachmentDimensionsAreTooBig'));
                            return;
                        }

                        if (!_.isUndefined(callback)) {
                            await callback();
                        }
                    }
                    image.src = e.target.result;
                };
                await reader.readAsDataURL(file.file);
            }
            else {
                if (!_.isUndefined(callback)) {
                    await callback();
                }
            }
        },
        openFileDialog() {
            const fu = this.$refs.upload
            fu.$el.querySelector("#" + fu.inputId).click()
        },
        async saveAttachment(attachment) {

            attachment.oldAttachmentId = attachment.attachmentId;
            var newAttachmentId = newGuid();
            attachment.attachmentId = newAttachmentId;

            await updateAttachment(this.questionnaireId, attachment);
        },
        cancel(attachment) {
            //reload from state
            attachment.name = this.initialName;
            this.file = [];
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
    },

}
</script>