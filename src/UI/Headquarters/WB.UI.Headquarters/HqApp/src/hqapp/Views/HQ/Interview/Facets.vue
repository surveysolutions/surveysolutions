<template>
    <div>
        <aside class="filters">
            <div class="wrapper-view-mode">
                <div class="foldback-button" id="hide-filters" @click="hideFacets">
                    <span class="arrow"></span>
                    <span class="arrow"></span>
                    <span class="glyphicon glyphicon-tasks" aria-hidden="true"></span>
                </div>
                <div class="filters-container">
                    <h2>{{$config.model.key}}</h2>
                    <ul class="list-unstyled about-questionnaire">
                        <li>
                            <strong>{{lastUpdateDate}}</strong>
                        </li>
                        <li>
                            <strong>{{ this.$t('Details.Responsible', { responsible: this.$config.model.responsible }) }}</strong>
                        </li>
                    </ul>
                    <div class="filter-actions-block">
                        <button type="button" class="btn btn-success" v-if="showApproveButton" @click="approve">
                            {{$t("Pages.ApproveRejectPartialView_ApproveAction")}}
                        </button>
                        <button type="button" class="btn btn-default btn-lg reject" v-if="showRejectButton" @click="reject">
                            {{$t("Pages.ApproveRejectPartialView_RejectAction")}}
                        </button>

                        <button type="button" class="btn btn-link" data-toggle="modal" data-target="#statusHistoryModal">History</button>
                    </div>
                </div>
                <SwitchLanguage />
                <FacetFilters />
                <button type="button" class="btn btn-link" @click="showSearchResults">show search results</button>
            </div>
        </aside>
        <Confirm ref="confirmApprove" id="confirmApprove" slot="modals" :title="$t('Pages.ApproveRejectPartialView_ApproveLabel')">
            <label for="txtApproveComment">
                {{$t("Pages.ApproveRejectPartialView_CommentLabel")}}:
            </label>
            <textarea class="form-control" rows="10" :maxlength="commentMaxLength" id="txtApproveComment" v-model="approveComment"></textarea>
            <span class="countDown">{{approveCharsLeft}}</span>
        </Confirm>
        <Confirm ref="rejectConfirm" id="rejectConfirm" slot="modals" :title="$t('Pages.ApproveRejectPartialView_RejectLabel')">
            <label for="txtApproveComment">
                {{$t("Pages.ApproveRejectPartialView_CommentLabel")}}:
            </label>
            <textarea class="form-control" rows="10" :maxlength="commentMaxLength" id="txtRejectComment" v-model="rejectComment"></textarea>
            <span class="countDown">{{rejectCharsLeft}}</span>
        </Confirm>
    </div>
</template>
<script>
import FacetFilters from "./FacetFilters";
import SwitchLanguage from "./SwitchLanguage";

export default {
  data() {
    return {
      approveComment: "",
      rejectComment: "",
      commentMaxLength: 1500
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
      this.$refs.rejectConfirm.promt(ok => {
        if (ok) {
          this.$store.dispatch("reject", this.reject).then(() => {
            window.location = this.$config.model.interviewsUrl;
          });
        }
      });
    },
    hideFacets() {
      this.$store.dispatch("hideFacets");
    },
    // temporaly to test panels
    showSearchResults() {
      this.$store.dispatch("showSearchResults");
    }
  },
  computed: {
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
    }
  },

  components: {
    FacetFilters,
    SwitchLanguage
  }
};
</script>
