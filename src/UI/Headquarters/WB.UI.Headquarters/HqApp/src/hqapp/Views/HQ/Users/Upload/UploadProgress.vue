<template>
    <div>
        <slot name="title">
            <h3>{{$t('UploadUsers.ImportingUserInfo')}} <br>{{fileName}}</h3>
        </slot>
        <div class="row">
            <div class="col-sm-7 col-xs-11 prefilled-data-info info-block">
                <p>
                    <h1>{{$t('UploadUsers.Uploading', {importedUsersCount: importedUsersCount, totalUsersToImportCount: totalUsersToImportCount})}}</h1>
                </p>
                <p>
                    <div class="cancelable-progress">
                        <div class="wrapper-progress">
                            <div class="progress" style="width:95%">
                                <div class="progress-bar progress-bar-primary" role="progressbar" v-bind:style="{ width: importedUsersInPercents + '%' }">
                                    <span class="sr-only">{{$t('UploadUsers.Uploading')}}</span>
                                </div>
                            </div>
                        </div>
                        <button type="button" class="btn btn-link" style="position:initial" @click="cancelUpload">
                            {{$t('UploadUsers.Cancel')}}
                        </button>
                    </div>
                </p>

            </div>
        </div>
    </div>
</template>


<script>
export default {
  data: function() {
    return {
      cancelled: false,
      timerId: 0
    };
  },
  computed: {
    config() {
      return this.$config.model;
    },
    progress() {
      return this.$store.getters.upload.progress;
    },
    fileName() {
      return this.$store.getters.upload.fileName;
    },
    importedUsersInPercents() {
      return this.importedUsersCount / this.progress.totalUsersToImport * 100;
    },
    importedUsersCount() {
      return this.totalUsersToImportCount - this.progress.usersInQueue;
    },
    totalUsersToImportCount() {
      return this.progress.totalUsersToImport;
    }
  },
  mounted() {
    this.updateStatus();
    this.timerId = window.setInterval(() => {
      this.updateStatus();
    }, 500);
  },
  methods: {
    cancelUpload() {
      window.clearInterval(this.timerId);

      var self = this;
      this.$http.post(this.config.api.importUsersCancelUrl).then(response => {
        self.$router.push({ name: "upload" });
      });
    },
    updateStatus() {
      var self = this;
      this.$http.get(this.config.api.importUsersStatusUrl).then(response => {
        this.$store.dispatch("setUploadStatus", response.data);

        if (response.data.usersInQueue == 0) {
          window.clearInterval(self.timerId);
          self.$http
            .get(this.config.api.importUsersCompleteStatusUrl)
            .then(response => {
              self.$store.dispatch("setUploadCompleteStatus", response.data);
              self.$router.push({ name: "uploadcomplete" });
            });
        }
      });
    }
  }
};
</script>
