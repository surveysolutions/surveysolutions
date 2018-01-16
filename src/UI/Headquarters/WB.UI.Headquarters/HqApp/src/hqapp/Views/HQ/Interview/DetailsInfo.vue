
<template>
    <div class="panel panel-details">
        <div class="panel-body clearfix">
            <div class="about-questionnaire clearfix">
                <h2>#{{$config.model.key}}</h2>
                <div class="about-questionnaire-details">
                    <h3>{{this.$store.state.webinterview.questionnaireTitle}}</h3>
                    <p>
                        {{this.$t('Details.Status', {name: this.$config.model.statusName })}}, {{this.$t('Details.Responsible')}}:
                        <a :class="responsibleRole" :href="this.$config.model.responsibleProfileUrl">
                            {{this.$config.model.responsible}}
                        </a>, {{lastUpdateDate}}
                        <button type="button" class="btn btn-link" @click="showStatusesHistory">{{$t("Common.ShowStatusHistory")}}</button>
                    </p>

                </div>
            </div>
            <div class="questionnaire-details-actions clearfix">
                <SwitchLanguage v-if="canChangeLanguage" />
                <button type="button" class="btn btn-success" v-if="showApproveButton" @click="approve">
                    {{$t("Pages.ApproveRejectPartialView_ApproveAction")}}
                </button>
                <button type="button" class="btn btn-default btn-lg reject" v-if="showRejectButton" @click="reject">
                    {{$t("Pages.ApproveRejectPartialView_RejectAction")}}
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

        <Confirm ref="rejectConfirm" id="rejectConfirm" slot="modals" :title="$t('Pages.ApproveRejectPartialView_RejectLAbel')" :disableOk="interviewerShouldbeSelected && !newResponsibleId">
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
    showRejectButton() {
      return (
        this.$config.model.approveReject.supervisorRejectAllowed ||
        this.$config.model.approveReject.hqOrAdminRejectAllowed
      );
    },
    lastUpdateDate() {
      return this.$t("Details.LastUpdated", {
        date: moment.utc(this.$config.model.lastUpdatedAtUtc).fromNow()
      });
    },
    canChangeLanguage() {
      return (
        this.$store.state.webinterview.languages != undefined &&
        this.$store.state.webinterview.languages.length > 0
      );
    }
  },

  components: {
    SwitchLanguage,
    StatusesHistory
  }
};
</script>

