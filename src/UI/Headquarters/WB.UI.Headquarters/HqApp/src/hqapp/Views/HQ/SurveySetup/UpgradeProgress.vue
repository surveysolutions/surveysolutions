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
        <div class="row-fluid" v-if="progress.progressDetails.status === 'Queued'">
            <div class="col-sm-12 prefilled-data-info info-block">
                {{$t('Assignments.UpgradePreparation')}}
            </div>
        </div>
        <div class="row-fluid" v-else-if="progress.progressDetails.status === 'InProgress'">
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
                    <button class="btn  btn-link" type="button" @click="stop">{{$t('Assignments.Stop')}}</button>
                </div>
            </div>
        </div>
        <div class="row-fluid" v-else-if="progress.progressDetails.status === 'Cancelled'">
            <div class="col-sm-12 prefilled-data-info info-block">
                {{$t('Assignments.UpgradeCancelled')}}
            </div>
        </div>
        <div class="row-fluid" v-else-if="progress.progressDetails.status === 'Done'">
            <div class="col-sm-7 col-xs-12 action-block preloading-done active-preloading">
                <div class="import-progress">
                    <p>
                        {{$t('Assignments.UpgradeProgressDoneCount', {processed: progress.progressDetails.assignmentsMigratedSuccessfully})}}
                    </p>
                    <p v-if="errorsCount">
                        {{$t('Assignments.UpgradeProgressErrorCount', {count: errorsCount})}}
                        <a :href="errorsExportUrl" target="_blank">({{$t('Assignments.UpgradeDownloadFailed')}})</a>
                    </p>
                </div>
                <div class="action-buttons">
                    <a class="btn btn-primary" :href="$config.model.surveySetupUrl">{{$t('MainMenu.SurveySetup')}}</a>
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
          status: "Queued",
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
    errorsExportUrl() {
      return `${this.$config.model.exportErrorsUrl}/${this.processId}`;
    },
    errorsCount() {
      return this.progress.progressDetails.assignmentsMigratedWithError.length;
    },
    totalProcessedCount() {
      return (
        this.progress.progressDetails.assignmentsMigratedSuccessfully +
        this.progress.progressDetails.assignmentsMigratedWithError.length
      );
    },
    overallProgressPercent() {
      return Math.round(
        this.totalProcessedCount /
          this.progress.progressDetails.totalAssignmentsToMigrate *
          100
      );
    }
  },
  methods: {
    updateStatus() {
      var self = this;
      this.$http
        .get(`${this.$config.model.progressUrl}/${this.processId}`)
        .then(
          response => {
            self.progress = response.data;
            if (
              self.progress.status === "Cancelled" ||
              self.progress.status === "Done"
            ) {
              window.clearInterval(self.timerId);
            }
          },
          error => {
            if (error.response.status === 404) {
              window.location = $config.model.surveySetupUrl;
            }
          }
        );
    },
    stop() {
      this.$http.post(`${this.$config.model.stopUrl}/${this.processId}`);
    }
  }
};
</script>

