<template>
    <div>
        {{processId}}
    </div>
</template>

<script>
export default {
  mounted() {
    this.updateStatus();
  },
  computed: {
    processId() {
      return this.$route.params.processId;
    }
  },
  methods: {
    updateStatus() {
      var self = this;
      this.$http
        .get(this.$config.model.progressUrl + "/" + this.processId)
        .then(response => {

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
    }
  }
};
</script>

