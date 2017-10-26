<template>
  <span style="display:none" :lastActivity="lastActivity" />
</template>

<script>
import modal from "./components/modal";

export default {
  computed: {
    lastActivity() {
      return this.$store.state.webinterview.lastActivityTimestamp;
    }
  },

  beforeMount() {
    setInterval(() => {
      if (!this.shown) {
        const minutesAfterLastAction = moment().diff(this.lastActivity, 'minutes');

        if (Math.abs(minutesAfterLastAction) > this.minutes) {
          this.show();
        }
      }
    }, 15 * 1000);
  },

  props: {
    minutes: {
      type: Number,
      default: 15
    }
  },

  data() {
    return {
      shown: false
    };
  },

  methods: {
    show() {
      if (this.shown) return;

      this.shown = true;
      this.$store.dispatch("stop");

      modal.alert({
        title: this.$t("WebInterviewUI.SessionTimeoutTitle"),
        message: `<p>${this.$t(
          "WebInterviewUI.SessionTimeoutMessageTitle"
        )}</p><p>${this.$t("WebInterviewUI.SessionTimeoutMessage")}</p>`,
        callback: () => {
          location.reload();
        },
        onEscape: false,
        closeButton: false,
        buttons: {
          ok: {
            label: this.$t("WebInterviewUI.Reload"),
            className: "btn-success"
          }
        }
      });
    }
  }
};
</script>
