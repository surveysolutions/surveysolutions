<template>
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
                <input type="button" :value="$t('QuestionnaireEditor.SideBarAttachmentsUpload')"
                    @click.stop="openFileDialog()" value="Upload new attachment" class="btn btn-default btn-lg pull-left"
                    :class="{ 'btn-primary': !isReadOnlyForUser }" v-if="!isReadOnlyForUser" capture />

                <file-upload ref="upload" v-if="!isReadOnlyForUser" :input-id="'tfunew'" v-model="file"
                    :size="100 * 1024 * 1024" :drop="false" :drop-directory="false" @input-file="createAndUploadFile"
                    accept=".pdf,image/*,video/*,audio/*">
                </file-upload>
            </div>
            <div class="empty-list" v-if="attachments.length == 0">
                <p> {{ $t('QuestionnaireEditor.SideBarAttachmentsEmptyLine1') }} </p>
                <p>
                    <span>{{ $t('QuestionnaireEditor.SideBarAttachmentsEmptyLine2') }}</span><span>&nbsp;</span>
                    <a href="https://support.mysurvey.solutions/questionnaire-designer/limits/multimedia-reference"
                        target="_blank">
                        {{ $t('QuestionnaireEditor.ClickHere') }}
                    </a>
                </p>
                <p v-dompurify-html="$t('QuestionnaireEditor.SideBarAttachmentsEmptyLine3', { name: variableNameHtml })" />
            </div>
            <form role="form" name="attachmentsForm" novalidate>
                <div class="attachment-list">
                    <template v-for="(attachment, index) in attachments">
                        <AttachmentItem :attachmentItem="attachment" :questionnaire-id="questionnaireId" />
                    </template>
                </div>
            </form>
        </perfect-scrollbar>
    </div>
</template>
  
<script>

import _ from 'lodash';
import moment from 'moment';
import { newGuid } from '../../../../helpers/guid';
import { notice } from '../../../../services/notificationService';
import AttachmentItem from './AttachmentItem.vue';
import { formatBytes } from '../../../../services/utilityService';
import { updateAttachment } from '../../../../services/attachmentsService';

export default {
    name: 'Attachments',
    inject: ['questionnaire', 'isReadOnlyForUser'],
    props: {
        questionnaireId: { type: String, required: true },
    },
    components: {
        AttachmentItem,
    },
    data() {
        return {
            benchmarkDownloadSpeed: 20,
            downloadLookupFileBaseUrl: '/attachments',
            allowedMaxResolution: 4096
        }
    },
    computed: {
        attachments() {
            return this.questionnaire.attachments;
        },
    },
    methods: {
        openFileDialog() {
            const fu = this.$refs.upload
            fu.$el.querySelector("#" + fu.inputId).click()
        },
        formatBytes(bytes) {
            return formatBytes(bytes);
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

        async updateAttachmentData(attachment, file, callback) {
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
                lastUpdateDate: null,
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

        async createAndUploadFile(file) {
            if (_.isNull(file) || _.isUndefined(file)) {
                return;
            }

            if (this.isReadOnlyForUser) {
                notice(this.$t('QuestionnaireEditor.NoPermissions'));
                return;
            }

            var attachment = {
                attachmentId: newGuid()
            };

            self = this;
            await this.updateAttachmentData(attachment, file, async () => {
                await updateAttachment(self.questionnaireId, attachment, true);
            });
        },
    }
}
</script>
  