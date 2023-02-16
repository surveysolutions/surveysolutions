<template>
  <section id="header" class="row">
    <a class="designer-logo" href="../../"></a>
    <div class="header-line">
        <div class="header-menu">
            <div class="buttons">
                <a class="btn" href="http://support.mysurvey.solutions/designer" target="_blank" v-i18next>Help</a>
                <a class="btn" href="https://forum.mysurvey.solutions" target="_blank" v-i18next>Forum</a>
            
                <a class="btn" href="../../questionnaire/questionnairehistory/{{questionnaire.questionnaireId}}" target="_blank"
                   v-if="questionnaire.hasViewerAdminRights || questionnaire.isSharedWithUser" v-i18next>History</a>
                <button class="btn" type="button" v-click="showDownloadPdf()" v-i18next>DownloadPdf</button>
                <a class="btn" type="button" v-show="questionnaire.hasViewerAdminRights" v-click="exportQuestionnaire()" v-i18next>SaveAs</a>

                <a class="btn"  v-if="questionnaire.questionnaireRevision || questionnaire.isReadOnlyForUser" href="../../questionnaire/clone/{{questionnaire.questionnaireId}}{{questionnaire.questionnaireRevision ? '$' + questionnaire.questionnaireRevision : ''}}" target="_blank" v-i18next>CopyTo</a>
                <button class="btn" type="button" v-disabled="questionnaire.isReadOnlyForUser && !questionnaire.hasViewerAdminRights && !questionnaire.isSharedWithUser" v-click="showShareInfo()" v-i18next>Settings</button>

                <div class="btn-group" v-if="currentUserIsAuthenticated">
                    <a class="btn btn-default" v-i18next="[i18next]({currentUserName: '{{ currentUserName }}' })HellowMessageBtn"></a>
                    <a class="btn btn-default dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                        <span class="caret"></span>
                        <span class="sr-only" v-i18next>ToggleDropdown</span>
                    </a>
                    <ul class="dropdown-menu dropdown-menu-right">
                        <li><a href="../../identity/account/manage" v-i18next>ManageAccount</a></li>
                        <li><a href="../../identity/account/logout" v-i18next>LogOut</a></li>
                    </ul>
                </div>
                <a class="btn" href="/" type="button" v-if="!currentUserIsAuthenticated" v-i18next>Login</a>
                <a class="btn" href="/identity/account/register" type="button" v-if="!currentUserIsAuthenticated" v-i18next>Register</a>
            </div>
        </div>
        <div class="questionnarie-title">
            <div class="title">
                <div class="questionnarie-title-text">
                    {{questionnaire.title}}
                </div>
                <div class="questionnarie-title-buttons">
                    <span class="text-muted" v-i18next="[i18next]({questionsCount: '{{questionnaire.questionsCount}}',
                                                                    groupsCount: '{{questionnaire.groupsCount}}',
                                                                    rostersCount: '{{questionnaire.rostersCount}}' })QuestionnaireSummary">

                    </span>
                    <input id="verification-btn" type="button" class="btn" v-i18next="[value]Compile" value="COMPILE" v-click="verify()" v-if="questionnaire.questionnaireRevision === null" />
                    <span v-show="verificationStatus.warnings!=null && verificationStatus.errors!=null">
                        <span data-toggle="modal" v-show="(verificationStatus.warnings.length + verificationStatus.errors.length) > 0" class="error-message v-hide" v-class="{'no-errors': verificationStatus.errors.length == 0}">
                            <a href="javascript:void(0);"
                               v-click="showVerificationErrors()"
                               v-i18next="[i18next]({count: verificationStatus.errors.length})ErrorsCounter">
                            </a>
                        </span>
                        <span data-toggle="modal" v-show="verificationStatus.warnings.length > 0" class="warniv-message v-hide">
                            <a href="javascript:void(0);"
                               v-click="showVerificationWarnings()"
                               v-i18next="[i18next]({count: verificationStatus.warnings.length})WarningsCounter">
                            </a>
                        </span>
                        <span class="text-success" v-show="(verificationStatus.warnings.length + verificationStatus.errors.length) === 0" v-i18next>Ok</span>

                        <span class="text-success"><em v-i18next="[i18next]({dateTime: verificationStatus.time})SavedAtTimestamp"></em></span>
                    </span>
                    <span class="error-message strong" v-show="questionnaire.isReadOnlyForUser" v-i18next>ReadOnly</span>
                    <input id="webtest-btn" type="button" class="btn" v-if="questionnaire.webTestAvailable && questionnaire.questionnaireRevision === null" v-i18next="[value]Test" value="Test" v-click="webTest()" />
                    <span class="error-message strong" v-show="questionnaire.previewRevision !== null && questionnaire.previewRevision !== undefined" v-i18next="({revision: questionnaire.previewRevision })Preview"></span>
                </div>
            </div>
        </div>
    </div>
  </section>
</template>

<script>
export default {
    name: 'Main',
    data() {},
};
</script>

