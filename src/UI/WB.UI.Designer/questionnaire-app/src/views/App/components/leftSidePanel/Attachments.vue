<template>
    Attachments
    <!--div class="attachments">
        <perfect-scrollbar class="scroller">
            <div class="panel-header clearfix">
                <div class="title pull-left">
                    <h3
                        ng-i18next="[i18next]({count: attachments.length, bytes: formatBytes(totalSize())})SideBarAttachmentsCounter" />
                    <p class="estimated-download-time"
                        ng-i18next="[i18next]({timeString: formatSeconds(estimatedLoadingTime()), downloadSpeed: benchmarkDownloadSpeed})SideBarAttachmentsEstimate">
                    </p>

                </div>
                <button class="btn btn-default btn-lg pull-left" :class="{ 'btn-primary': !isReadOnlyForUser }" ngf-select
                    ngf-change="createAndUploadFile($file);$event.stopPropagation()"
                    ngf-accept="'.pdf,image/*,video/*,audio/*'" ngf-max-size="100MB" type="file"
                    ngf-select-disabled="isReadOnlyForUser" ngf-drop-disabled="isReadOnlyForUser"
                    ng-disabled="isReadOnlyForUser" ng-i18next="SideBarAttachmentsUpload">
                    Upload new
                </button>
            </div>
            <div class="empty-list" ng-show="attachments.length == 0">
                <p ng-i18next="SideBarAttachmentsEmptyLine1"></p>
                <p>
                    <span ng-i18next="SideBarAttachmentsEmptyLine2"></span>
                    <a href="https://support.mysurvey.solutions/questionnaire-designer/limits/multimedia-reference"
                        target="_blank" ng-i18next="ClickHere">
                        click here
                    </a>
                </p>
                <p ng-bind-html="emptyAttachmentsDescription" />
            </div>
            <form role="form" name="attachmentsForm" novalidate>
                <div class="attachment-list">
                    <ng-form name="attachment.form" ng-repeat="attachment in attachments">
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
                                            ng-src='{{downloadLookupFileBaseUrl + "/" + questionnaire.questionnaireId + "/thumbnail/" + attachment.attachmentId}}'>
                                    </div>
                                </div>
                                <div class="attachment-content">
                                    <input focus-on-out="focusAttachment{{attachment.attachmentId}}" required=""
                                        ng-i18next="[placeholder]SideBarAttachmentName" maxlength="32" spellcheck="false"
                                        ng-model="attachment.name" name="name" class="form-control table-name"
                                        type="text" />
                                    <div class="divider"></div>
                                    <div class="drop-box" ng-i18next="SideBarLookupTableDropFile">
                                        Drop File here
                                    </div>
                                    <div class="attachment-meta" ng-include="'Image-attachment-info-template.html'"></div>
                                    <div class="actions clearfix" :class="{ dirty: attachment.form.$dirty }">
                                        <div ng-show="attachment.form.$dirty" class="pull-left">
                                            <button type="submit"
                                                ng-disabled="questionnaire.isReadOnlyForUser || attachment.form.$invalid"
                                                class="btn lighter-hover"
                                                @click="saveAttachment(attachment); $event.stopPropagation()"
                                                ng-i18next>Save</button>
                                            <button type="button" class="btn lighter-hover"
                                                @click="cancel(attachment); $event.stopPropagation()"
                                                ng-i18next>Cancel</button>
                                        </div>
                                        <div class="permanent-actions pull-right clearfix">
                                            <button ng-disabled="isReadOnlyForUser" class="btn btn-default pull-right"
                                                ngf-select="" ngf-accept="'.pdf,image/*,video/*,audio/*'"
                                                ngf-max-size="100MB"
                                                ngf-change="fileSelected(attachment, $file);$event.stopPropagation()"
                                                type="file">
                                                <span ng-i18next>Update</span>
                                            </button>
                                            <a href="{{downloadLookupFileBaseUrl + '/' + questionnaire.questionnaireId + '/' + attachment.attachmentId}}"
                                                class="btn btn-default pull-right" target="_blank" rel="noopener noreferrer"
                                                ng-i18next>Download</a>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </ng-form>
                </div>
            </form>
        </perfect-scrollbar>
    </div-->
</template>
  
<script>
export default {
    name: 'Attachments',
    props: {},
    data() {
        return {

        }
    }
}
</script>
  