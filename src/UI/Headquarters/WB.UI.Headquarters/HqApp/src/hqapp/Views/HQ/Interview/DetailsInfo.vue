
<template>
    <div class="panel panel-details" :class="{ 'contains-action-buttons': showRejectButton || showUnapproveButton || showApproveButton, 'contains-tranlation' : canChangeLanguage}">
        <div class="panel-body clearfix">
            <div class="about-questionnaire clearfix">
                <div class="about-questionnaire-details clearfix">
                    <ul class="main-info-column list-unstyled pull-left">
                        <li>{{this.$t('Common.InterviewKey')}}: {{$config.model.key}}({{this.$t('Common.Assignment')}} #{{this.$config.model.assignmentId}})</li>
                        <li class="questionnaire-title">[ver.{{this.$config.model.questionnaireVersion}}] {{this.$config.model.questionnaireTitle}}</li>
                    </ul>
                    <ul class="list-unstyled pull-left table-info">
                        <li v-if="this.$config.model.interviewDuration">
                            <span class="data-label">{{this.$t('Details.Duration')}}:</span>
                            <span class="data">{{this.$config.model.interviewDuration}}</span>
                        </li>
                        <li>
                            <span class="data-label">{{this.$t('Details.Responsible')}}: </span>
                            <span v-if="isInterviewerResponsible" class="data">
                                <a :class="responsibleRole" :href="this.$config.model.responsibleProfileUrl">{{this.$config.model.responsible}}</a>
                            </span>
                            <span v-else class="data supervisor">{{this.$config.model.responsible}}</span>
                        </li>
                        <li>
                            <span class="data-label">{{this.$t('Users.Supervisor')}}: </span>
                            <span class="data supervisor">{{this.$config.model.supervisor}}</span>
                        </li>
                    </ul>
                    <ul class="list-unstyled pull-left table-info">
                        <li><span class="data-label">{{this.$t('Details.Status')}}</span> 
                            <span class="data">{{this.$config.model.statusName}}</span>
                            <button type="button" class="btn btn-link gray-action-unit" @click="showStatusesHistory">{{$t("Common.ShowStatusHistory")}}</button>
                            
                        </li>
                        <li><span class="data-label">{{this.$t('Details.LastUpdated')}}:</span> 
                            <span class="data">{{lastUpdateDate}}</span>                            
                            <button type="button" class="btn btn-link gray-action-unit" @click="showOverview">{{$t("Details.Overview")}}</button>
                        </li>
                    </ul>
                </div>
            </div>
            <div class="questionnaire-details-actions clearfix" >
                <SwitchLanguage v-if="canChangeLanguage" :disabled="changeLanguageDisabled"/>
                <button type="button" class="btn btn-success" v-if="showApproveButton" @click="approve" :disabled="changeStatusDisabled">
                    {{$t("Pages.ApproveRejectPartialView_ApproveAction")}}
                </button>
                <button type="button" class="btn btn-default btn-lg reject" v-if="showRejectButton" @click="reject" :disabled="changeStatusDisabled">
                    {{$t("Pages.ApproveRejectPartialView_RejectAction")}}
                </button>
                <button type="button" class="btn btn-default btn-lg reject" v-if="showUnapproveButton" @click="reject">
                    {{$t("Pages.ApproveRejectPartialView_UnapproveAction")}}
                </button>
            </div>
        </div>
        <StatusesHistory ref="statusesHistory" id="statusesHistory" slot="modals" class="statusHistoryModal" />
        <Confirm ref="confirmApprove" id="confirmApprove" slot="modals" :title="$t('Pages.ApproveRejectPartialView_ApproveLabel')">
            <label for="txtApproveComment">
                {{$t("Pages.ApproveRejectPartialView_CommentLabel")}}:
            </label>
            <textarea class="form-control" rows="10" :maxlength="commentMaxLength" id="txtApproveComment" v-model="approveComment"></textarea>
            <span class="countDown">{{approveCharsLeft}}</span>
        </Confirm>

        <Confirm ref="rejectConfirm" id="rejectConfirm" slot="modals" 
                :title="showUnapproveButton ? $t('Pages.ApproveRejectPartialView_UnapproveLabel') : $t('Pages.ApproveRejectPartialView_RejectLAbel')" 
                :disableOk="interviewerShouldbeSelected && !newResponsibleId">
            <form v-if="interviewerShouldbeSelected" onsubmit="return false;">
                <div class="form-group">
                    <label class="control-label" for="newResponsibleId">{{ $t("Details.ChooseResponsibleInterviewer") }}</label>
                    <Typeahead :placeholder="$t('Common.Responsible')" control-id="newResponsibleId" :value="newResponsibleId" @selected="newResponsibleSelected" :fetch-url="this.$config.model.approveReject.interviewersListUrl">
                    </Typeahead>
                </div>
            </form>

            <label for="txtApproveComment">
                {{$t("Pages.ApproveRejectPartialView_CommentLabel")}}:
            </label>
            <textarea class="form-control" rows="10" :maxlength="commentMaxLength" id="txtRejectComment" v-model="rejectComment"></textarea>
            <span class="countDown">{{rejectCharsLeft}}</span>
        </Confirm>
    </div>
</template>

<script>
import SwitchLanguage from "./SwitchLanguage";
import StatusesHistory from "./StatusesHistory";
import Vue from "vue";

export default {
  data() {
    return {
      approveComment: "",
      rejectComment: "",
      commentMaxLength: 1500,
      newResponsibleId: null
    };
  },
  methods: {
    approve() {
      this.$refs.confirmApprove.promt(ok => {
        if (ok) {
          this.$store.dispatch("approve", this.approveComment).then(() => {
            window.location = this.$config.model.interviewsUrl;
          });
        }
      });
    },
    reject() {
      this.$refs.rejectConfirm.promt(async ok => {
        if (ok) {
          var newId = (this.newResponsibleId || {}).key;
          var dispatchResult = this.$store.dispatch("reject", {
            comment: this.rejectComment,
            assignTo: newId
          });
          dispatchResult.then(() => {
            window.location = this.$config.model.interviewsUrl;
          });
        }
      });
    },
    newResponsibleSelected(newValue) {
      this.newResponsibleId = newValue;
    },
    showStatusesHistory() {
      this.$refs.statusesHistory.show();
    },
    showOverview() {
        this.$router.push({name: "Overview"})
    }
  },
  
  computed: {
    responsibleRole() {
      return this.$config.model.responsibleRole.toLowerCase();
    },
    interviewerShouldbeSelected() {
      return this.$config.model.approveReject.interviewerShouldbeSelected;
    },
    approveCharsLeft() {
      return `${this.approveComment.length} / ${this.commentMaxLength}`;
    },
    rejectCharsLeft() {
      return `${this.rejectComment.length} / ${this.commentMaxLength}`;
    },
    showApproveButton() {      
      return (
        this.$config.model.approveReject.supervisorApproveAllowed ||
        this.$config.model.approveReject.hqOrAdminApproveAllowed
      );
    },
    showUnapproveButton() {
        return this.$config.model.approveReject.hqOrAdminUnapproveAllowed;
    },
    showRejectButton() {      
      return (
        this.$config.model.approveReject.supervisorRejectAllowed ||
        this.$config.model.approveReject.hqOrAdminRejectAllowed
      );
    },
    lastUpdateDate() {
      return moment.utc(this.$config.model.lastUpdatedAtUtc).fromNow();      
    },
    canChangeLanguage() {
      return (
        this.$store.state.webinterview.languages != undefined &&
        this.$store.state.webinterview.languages.length > 0
      );      
    },
    changeLanguageDisabled() {
      return this.$store.state.webinterview.interviewCannotBeChanged;
    },
    changeStatusDisabled() {
      return this.$store.state.webinterview.isCurrentUserObserving;
    },
    isInterviewerResponsible() {
      return this.$config.model.responsibleRole == "Interviewer";
    }
  },

  components: {
    SwitchLanguage,
    StatusesHistory
  }
};
</script>

