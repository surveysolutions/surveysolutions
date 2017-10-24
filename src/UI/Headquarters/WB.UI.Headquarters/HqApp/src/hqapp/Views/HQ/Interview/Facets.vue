<template>
<div>
    <aside class="filters">
        <div class="wrapper-view-mode">
            <div class="foldback-button"
                 id="hide-filters">
                <span class="arrow"></span>
                <span class="arrow"></span>
                <span class="glyphicon glyphicon-tasks"
                      aria-hidden="true"></span>
            </div>
            <div class="filters-container">
                <h2>{{$config.model.key}}</h2>
                <ul class="list-unstyled about-questionnaire">
                    <li><strong>{{lastUpdateDate}}</strong></li>
                    <li><strong>{{ this.$t('WebInterviewUI.Responsible', { responsible: this.$config.model.responsible }) }}</strong></li>
                </ul>
                <div class="filter-actions-block">
                    <button type="button"
                            class="btn btn-success" v-if="showApproveButton"
                            @click="approve">
                            {{$t("Pages.ApproveRejectPartialView_ApproveAction")}}
                    </button>
                    <button type="button"
                            class="btn btn-default btn-lg reject" v-if="showRejectButton">{{$t("Pages.ApproveRejectPartialView_RejectAction")}}</button>

                    <button type="button"
                            class="btn btn-link"
                            data-toggle="modal"
                            data-target="#statuses">History</button>
                </div>
            </div>
            <FacetFilters />
        </div>
    </aside>
    <Confirm ref="confirmApprove"
             id="confirmApprove"
             slot="modals"
             :title="$t('Pages.ApproveRejectPartialView_ApproveLabel')">
             <label for="txtApproveComment">
                 {{$t("Pages.ApproveRejectPartialView_CommentLabel")}}:
             </label>
             <textarea class="form-control" rows="10" :maxlength="commentMaxLength"
                id="txtApproveComment" v-model="approveComment"></textarea>
            <span class="countDown">{{approveCharsLeft}}</span>
    </Confirm>
</div>
</template>
<script>

import FacetFilters from "./FacetFilters"

export default {
  data() {
    return {
      approveComment: "",
      commentMaxLength: 1500
    };
  },
  methods: {
    approve() {
      this.$refs.confirmApprove.promt(ok => {
        if (ok) {
          var action = this.$config.model.approveReject.hqOrAdminApproveAllowed
            ? "hqApprove"
            : "superviorApprove";
          this.$store.dispatch(action, this.approveComment).then(() => {
            window.location = this.$config.model.interviewsUrl;
          });
        }
      });
    }
  },
  computed: {
    approveCharsLeft() {
      return `${this.approveComment.length} / ${this.commentMaxLength}`;
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
      return this.$t("WebInterviewUI.LastUpdated", {
        date: moment.utc(this.$config.model.lastUpdatedAtUtc).fromNow()
      });
    }
  },

  components: {
      FacetFilters
  }
};
</script>
