<template>
  <section id="header" class="row">
    <a class="designer-logo" href="../../"></a>
    <div class="header-line">
      <div class="header-menu">
        <div class="buttons">
          <a class="btn" href="http://support.mysurvey.solutions/designer" target="_blank">{{
            $t('QuestionnaireEditor.Help') }}</a>
          <a class="btn" href="https://forum.mysurvey.solutions" target="_blank">{{ $t('QuestionnaireEditor.Forum') }}
          </a>
          <a class="btn" href="../../questionnaire/questionnairehistory/{{questionnaire.questionnaireId}}" target="_blank"
            v-if="questionnaire.hasViewerAdminRights ||
              questionnaire.isSharedWithUser
              ">{{ $t('QuestionnaireEditor.History') }}</a>
          <button class="btn" type="button" @click="showDownloadPdf()">
            {{ $t('QuestionnaireEditor.DownloadPdf') }}
          </button>
          <a class="btn" type="button" v-if="questionnaire.hasViewerAdminRights" @click="ortQuestionnaire()">{{
            $t('QuestionnaireEditor.SaveAs') }}</a>

          <a class="btn" v-if="questionnaire.questionnaireRevision ||
            questionnaire.isReadOnlyForUser
            "
            href="../../questionnaire/clone/{{questionnaire.questionnaireId}}{{questionnaire.questionnaireRevision ? '$' + questionnaire.questionnaireRevision : ''}}"
            target="_blank">{{ $t('QuestionnaireEditor.CopyTo') }}</a>
          <button class="btn" type="button" v-disabled="questionnaire.isReadOnlyForUser &&
            !questionnaire.hasViewerAdminRights &&
            !questionnaire.isSharedWithUser
            " @click="showShareInfo()">
            {{ $t('QuestionnaireEditor.Settings') }}
          </button>

          <div class="btn-group" v-if="currentUserIsAuthenticated">
            <!-- <a class="btn btn-default">{{ $t('QuestionnaireEditor.HellowMessageBtn', {currentUserName:currentUserName}) }}</a> -->
            <a class="btn btn-default dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
              <span class="caret"></span>
              <span class="sr-only">{{
                $t('QuestionnaireEditor.ToggleDropdown')
              }}</span>
            </a>
            <ul class="dropdown-menu dropdown-menu-right">
              <li>
                <a href="../../identity/account/manage">{{
                  $t('QuestionnaireEditor.ManageAccount')
                }}</a>
              </li>
              <li>
                <a href="../../identity/account/logout">{{
                  $t('QuestionnaireEditor.LogOut')
                }}</a>
              </li>
            </ul>
          </div>
          <a class="btn" href="/" type="button" v-if="!currentUserIsAuthenticated">{{ $t('QuestionnaireEditor.Login')
          }}</a>
          <a class="btn" href="/identity/account/register" type="button" v-if="!currentUserIsAuthenticated">{{
            $t('QuestionnaireEditor.Register') }}</a>
        </div>
      </div>
      <div class="questionnarie-title">
        <div class="title">
          <div class="questionnarie-title-text">
            {{ questionnaire.title }}
          </div>
          <div class="questionnarie-title-buttons">
            <!-- <span class="text-muted">{{ $t('QuestionnaireEditor.QuestionnaireSummary', { questionsCount: questionnaire.questionsCount, groupsCount: questionnaire.groupsCount, rostersCount: questionnaire.rostersCount}) }} 

                    </span>-->
            <button id="verification-btn" type="button" class="btn" @click="verify()" v-if="questionnaire.questionnaireRevision === null
              ">
              {{ $t('QuestionnaireEditor.Compile') }}
            </button>
            <span v-if="verificationStatus.warnings != null &&
              verificationStatus.errors != null
              ">
              <span data-toggle="modal" v-if="verificationStatus.warnings.length +
                verificationStatus.errors.length >
                0
                " class="error-message v-hide" v-class="{
    'no-errors':
      verificationStatus.errors.length ==
      0
  }">
                <!-- <a href="javascript:void(0);"
                               @click="showVerificationErrors()">{{ $t('QuestionnaireEditor.ErrorsCounter',{ count: verificationStatus.errors.length}) }}
                            </a> -->
              </span>
              <span data-toggle="modal" v-if="verificationStatus.warnings.length > 0
                " class="warniv-message v-hide">
                <a href="javascript:void(0);" @click="showVerificationWarnings()">
                  <!-- {{ $t('QuestionnaireEditor.WarningsCounter',{ count: verificationStatus.warnings.length}) }} -->
                </a>
              </span>
              <span class="text-success" v-if="verificationStatus.warnings.length +
                verificationStatus.errors.length ===
                0
                ">{{ $t('QuestionnaireEditor.Ok') }}</span>

              <span class="text-success">
                <!-- <em>{{ $t('QuestionnaireEditor.SavedAtTimestamp',{ dateTime: verificationStatus.time}) }}</em> -->
              </span>
            </span>
            <span class="error-message strong" v-if="questionnaire.isReadOnlyForUser">{{
              $t('QuestionnaireEditor.ReadOnly') }}</span>
            <button id="webtest-btn" type="button" class="btn" v-if="questionnaire.webTestAvailable &&
              questionnaire.questionnaireRevision ===
              null
              " @click="webTest()">
              {{ $t('QuestionnaireEditor.Test') }}
            </button>
          </div>
        </div>
      </div>
    </div>
  </section>

  <section id="spacer" class="row">
    <div class="left"></div>
    <div class="right"></div>
  </section>
</template>


<!--script setup>
import { useQuestionnaireStore } from '../../../stores/questionnaire'
const questionnaireStore = useQuestionnaireStore()
</script-->
<script>

import { useQuestionnaireStore } from '../../../stores/questionnaire'

export default {
  name: 'QuestionnaireHeader',
  //inject: ["useQuestionnaireStore"],
  props: {},
  data() {
    return {
      questionnaire: {
        questionnaireId: 0,
        hasViewerAdminRights: false,
        isSharedWithUser: false,
        questionnaireRevision: null,
        isReadOnlyForUser: false,
        webTestAvailable: false,
        title: '',
        questionsCount: 0,
        groupsCount: 0,
        rostersCount: 0,

      },
      verificationStatus: {
        warnings: [],
        errors: [],
        time: ''
      },
    }
  },
  setup(props) {
    debugger
    const questionnaireStore = useQuestionnaireStore()
    questionnaireStore.fetchQuestionnaireInfo()
  },
  beforeMount() {
    this.fetch()
  },
  methods: {
    fetch() {
      //questionnaireStore.alert()
      //this.questionnaireStore.fetchQuestionnaireInfo()
    },
  }
}
</script>
