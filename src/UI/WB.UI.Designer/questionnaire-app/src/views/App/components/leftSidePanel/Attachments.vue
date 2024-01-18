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
                <button class="btn btn-default btn-lg pull-left" :class="{ 'btn-primary': !isReadOnlyForUser }" ngf-select
                    ngf-change="createAndUploadFile($file);$event.stopPropagation()"
                    ngf-accept="'.pdf,image/*,video/*,audio/*'" ngf-max-size="100MB" type="file"
                    ngf-select-disabled="isReadOnlyForUser" ngf-drop-disabled="isReadOnlyForUser"
                    ng-disabled="isReadOnlyForUser" ng-i18next="SideBarAttachmentsUpload">
                    {{ $t('QuestionnaireEditor.SideBarAttachmentsUpload') }}
                </button>
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
                    <ng-form name="attachment.form" v-for="attachment in attachments">
                        <div class="attachments-panel-item" :class="{ 'has-error': attachment.form.name.$error.pattern }"
                            ngf-drop="" ngf-max-size="100MB" ngf-change="fileSelected(attachment, $file)"
                            ngf-drag-over-class="{accept:'dragover', reject:'dragover-err'}">
                            <a href @click="deleteAttachment($index)" ng-disabled="questionnaire.isReadOnlyForUser"
                                ng-if="!questionnaire.isReadOnlyForUser" class="btn delete-btn" tabindex="-1"></a>
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
                                        ng-i18next="[placeholder]SideBarAttachmentName" maxlength="32" spellcheck="false"
                                        v-model="attachment.name" name="name" class="form-control table-name" type="text" />
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
                                    <div class="actions clearfix" :class="{ dirty: attachment.form.$dirty }">
                                        <div ng-show="attachment.form.$dirty" class="pull-left">
                                            <button type="submit"
                                                :disabled="questionnaire.isReadOnlyForUser || attachment.form.$invalid"
                                                class="btn lighter-hover" @click="saveAttachment(attachment)">{{
                                                    $t('QuestionnaireEditor.Save') }}</button>
                                            <button type="button" class="btn lighter-hover" @click="cancel(attachment)">{{
                                                $t('QuestionnaireEditor.Cancel') }}</button>
                                        </div>
                                        <div class="permanent-actions pull-right clearfix">
                                            <button ng-disabled="isReadOnlyForUser" class="btn btn-default pull-right"
                                                ngf-select="" ngf-accept="'.pdf,image/*,video/*,audio/*'"
                                                ngf-max-size="100MB"
                                                ngf-change="fileSelected(attachment, $file);$event.stopPropagation()"
                                                type="file">
                                                <span>{{ $t('QuestionnaireEditor.Update') }}</span>
                                            </button>
                                            <a :href="downloadLookupFileBaseUrl + '/' + questionnaire.questionnaireId + '/' + attachment.attachmentId"
                                                class="btn btn-default pull-right" target="_blank"
                                                rel="noopener noreferrer">{{ $t('QuestionnaireEditor.Download') }}</a>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </ng-form>
                </div>
            </form>
        </perfect-scrollbar>
    </div>
</template>
  
<script>

import _ from 'lodash';
import moment from 'moment';

export default {
    name: 'Attachments',
    props: {},
    data() {
        return {
            benchmarkDownloadSpeed: 20,
            attachments: [],
            downloadLookupFileBaseUrl: '../../attachments',
        }
    },
    methods: {
        formatBytes(bytes) {
            if (bytes === 0) return '0 Byte';
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
    }
}
</script>
  