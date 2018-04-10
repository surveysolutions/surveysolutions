<template>
    <HqLayout :hasFilter="false" :title="$t('Pages.UpgradeAssignmentsTitle')">
        <div slot="headers">
            <ol class="breadcrumb">
                <li>
                    <a :href="$config.model.surveySetupUrl">{{$t('MainMenu.SurveySetup')}}</a>
                </li>
            </ol>
            <h1>
                {{$t('Assignments.UpgradeProgressTitle', {to: progress.migrateToTitle, from: progress.migrateFromTitle})}}
            </h1>
        </div>
        <div class="row-fluid">
            <div class="col-sm-7 col-xs-12 action-block uploading-verifying active-preloading">
                <div class="import-progress">
                    <p>
                        {{ $t('Assignments.UpgradeProgressNumbers', { processed: totalProcessedCount, totalCount: progress.progressDetails.totalAssignmentsToMigrate }) }}
                    </p>
                </div>
                <div class="cancelable-progress">
                    <div class="progress">
                        <div class="progress-bar" role="progressbar" aria-valuenow="60" aria-valuemin="0" aria-valuemax="100" v-bind:style="{ width: overallProgressPercent + '%' }">
                            <span class="sr-only">{{overallProgressPercent}}%</span>
                        </div>
                    </div>
                    <button class="btn  btn-link" type="button" @click="stop">{{$t('UploadUsers.Cancel')}}</button>
                </div>
            </div>
        </div>

    </HqLayout>

</template>

<script>
export default {
  data() {
    return {
      progress: {
        progressDetails: {
          assignmentsMigratedWithError: []
        }
      }
    };
  },
  mounted() {
    this.updateStatus();
    this.timerId = window.setInterval(() => {
      this.updateStatus();
    }, 3000);
  },
  computed: {
    processId() {
      return this.$route.params.processId;
    },
    totalProcessedCount() {
      return (
        this.progress.progressDetails.assignmentsMigratedSuccessfully +
        this.progress.progressDetails.assignmentsMigratedWithError.length
      );
    },
    overallProgressPercent() {
      return (
        Math.round(
          this.totalProcessedCount /
            this.progress.progressDetails.totalAssignmentsToMigrate
        ) * 100
      );
    }
  },
  methods: {
    updateStatus() {
      var self = this;
      this.$http
        .get(`${this.$config.model.progressUrl}/${this.processId}`)
        .then(response => {
          self.progress = response.data;

          // if (response.data.usersInQueue == 0) {
          //   window.clearInterval(self.timerId);
          //   self.$http
          //     .get(this.config.api.importUsersCompleteStatusUrl)
          //     .then(response => {
          //       self.$store.dispatch("setUploadCompleteStatus", response.data);
          //       self.$router.push({ name: "uploadcomplete" });
          //     });
          // }
        });
    },
    stop() {
        this.$http.post(`${this.$config.model.stopUrl}/${this.processId}`)
    }
  }
};
</script>

