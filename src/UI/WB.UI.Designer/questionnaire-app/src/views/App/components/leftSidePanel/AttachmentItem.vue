<template>
    <div name="attachment.form">
        <div class="attachments-panel-item" :class="{ 'has-error': isNameValid }">
            <a href="javascript:void(0);" @click="deleteAttachment(attachment)" v-if="!isReadOnlyForUser"
                class="btn delete-btn" tabindex="-1"></a>
            <div class="attachment">
                <div class="attachment-preview">
                    <div class="attachment-preview-cover clearfix">
                        <img class="pull-right" @click="previewAttachment(attachment)" style="{width: 156, height: 140}"
                            v-if="!attachment.file"
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
                        <p v-if="attachment.meta.lastUpdateDate">
                            {{ $t('QuestionnaireEditor.SideBarAttachmentUploaded', {
                                lastUpdate: attachment.meta.lastUpdateDate
                            }) }}
                        </p>
                    </div>
                    <div class="actions clearfix" :class="{ 'dirty': isDirty }">
                        <div v-if="isDirty" class="pull-left">
                            <button type="button" v-if="!isReadOnlyForUser" :disabled="isInvalid ? 'disabled' : null"
                                class="btn lighter-hover" @click="saveAttachment(attachment)">{{
                                    $t('QuestionnaireEditor.Save') }}</button>
                            <button type="button" class="btn lighter-hover" @click="cancel(attachment)">{{
                                $t('QuestionnaireEditor.Cancel') }}</button>
                        </div>
                        <div class="permanent-actions pull-right clearfix">
                            <button type="button" :value="$t('QuestionnaireEditor.SideBarAttachmentsUpload')"
                                @click.stop="openFileDialog()" value="Upload new attachment"
                                class="btn btn-default pull-right" v-if="!isReadOnlyForUser" capture>
                                <span>{{ $t('QuestionnaireEditor.Update') }}</span>
                            </button>

                            <file-upload ref="upload" v-if="!isReadOnlyForUser" :input-id="'tfunew'" v-model="file"
                                :size="100 * 1024 * 1024" :drop="false" :drop-directory="false" @input-file="fileSelected"
                                accept=".pdf,image/*,video/*,audio/*">
                            </file-upload>

                            <a :href="getDownloadUrl()" class="btn btn-default pull-right" target="_blank"
                                rel="noopener noreferrer">{{
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
        attachmentItem: { type: Object, required: true },
        questionnaireId: { type: String, required: true },
    },
    inject: ['questionnaire', 'isReadOnlyForUser'],
    data() {
        return {
            downloadBaseUrl: '/attachments',
            file: []
        }
    },
    computed: {
        attachment() {
            return this.attachmentItem.editAttachment;
        },
        isDirty() {
            return this.attachment.name != this.attachmentItem.name || (this.attachment.file !== null && this.attachment.file !== undefined);
        },
        isInvalid() {
            return (this.attachment.name) ? false : true;
        },
    },
    methods: {
        formatBytes(bytes) {
            return formatBytes(bytes);
        },
        previewAttachment() {
            if (this.attachment.file !== null && this.attachment.file !== undefined)
                return;
            var srcImage = this.attachment.file || (this.downloadBaseUrl + "/" + this.questionnaireId + "/thumbnail/" + this.attachment.attachmentId + '/568');
            var confirmParams = {
                noControls: true,
                header: this.$t('QuestionnaireEditor.AttachmentPreview'),
                title: `<img class="attachment-preview-img" size="{width: 568, height: 568}" src="${srcImage}">`,
            }

            this.$confirm(confirmParams);
        },
        getDownloadUrl() {
            return this.attachment ? this.downloadBaseUrl + '/' + this.questionnaire.questionnaireId + '/' + this.attachment.attachmentId : '';
        },
        isAttachmentSizeTooBig() {
            return this.attachment.content.size > 5 * 1024 * 1824;
        },
        isAttachmentResolutionTooBig() {
            var recommendedMaxResolution = 1024;
            return ((this.attachment.content.details.height || 0) > recommendedMaxResolution) || ((this.attachment.content.details.width || 0) > recommendedMaxResolution);
        },
        isNameValid() {
            return true;
        },
        //TODO move to reuse
        async fileSelected(file) {
            if (_.isNull(file) || _.isUndefined(file)) {
                return;
            }

            if (this.isReadOnlyForUser) {
                notice(this.$t('QuestionnaireEditor.NoPermissions'));
                return;
            }

            this.attachment.file = file.file;

            this.attachment.content = {
                size: file.size,
                type: file.type,
                details: {}
            };
            this.attachment.meta = {
                lastUpdateDate: null,
                fileName: file.name
            };

            if (this.attachment.meta.fileName) {
                var maxAttachmentNameLength = 32;
                var attachmentFileNameLength = this.attachment.meta.fileName.length;

                this.attachment.name = this.attachment.meta.fileName.replace(/\.[^/.]+$/, "")
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

                        self.attachment.content.details.height = height;
                        self.attachment.content.details.width = width;

                        if (((self.attachment.content.details.height || 0) > self.allowedMaxResolution)
                            || ((self.attachment.content.details.width || 0) > self.allowedMaxResolution)) {
                            notice(self.$t('QuestionnaireEditor.AttachmentDimensionsAreTooBig'));
                            return;
                        }
                    }
                    image.src = e.target.result;
                };
                await reader.readAsDataURL(file.file);
            }
        },
        openFileDialog() {
            const fu = this.$refs.upload
            fu.$el.querySelector("#" + fu.inputId).click()
        },
        async saveAttachment() {
            this.attachment.oldAttachmentId = this.attachment.attachmentId;
            this.attachment.attachmentId = newGuid();

            await updateAttachment(this.questionnaireId, this.attachment);

            this.attachment.file = null;
            this.file = [];
        },
        cancel() {
            var clonned = _.cloneDeep(this.attachmentItem);
            clonned.editAttachment = null;
            this.attachmentItem.editAttachment = clonned;
        },
        deleteAttachment() {
            var attachmentName = this.attachment.name || this.$t('QuestionnaireEditor.SideBarAttachmentName');

            const questionnaireId = this.questionnaire.questionnaireId;
            const attachmentId = this.attachment.attachmentId;
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