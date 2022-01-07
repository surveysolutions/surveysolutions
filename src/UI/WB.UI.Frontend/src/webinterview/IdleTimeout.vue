<template>
    <span style="display:none"
        :lastActivity="lastActivity" />
</template>

<script>
import modal from '@/shared/modal'
import moment from'moment'

export default {
    computed: {
        lastActivity() {
            return this.$store.state.webinterview.lastActivityTimestamp
        },
    },

    beforeMount() {
        var self = this
        setInterval(() => {
            if (!self.shown) {
                const minutesAfterLastAction = moment().diff(self.lastActivity, 'minutes')

                if (Math.abs(minutesAfterLastAction) >= self.minutes) {
                    self.show()
                }
            }
        }, 15 * 1000)
    },

    props: {
        minutes: {
            type: Number,
            default: 15,
        },
    },

    data() {
        return {
            shown: false,
        }
    },

    methods: {
        show() {
            if (this.shown) return

            this.shown = true
            this.$store.dispatch('stop')

            modal.dialog({
                title: this.$t('WebInterviewUI.SessionTimeoutTitle'),
                message: `<p>${this.$t(
                    'WebInterviewUI.SessionTimeoutMessageTitle'
                )}</p><p>${this.$t('WebInterviewUI.SessionTimeoutMessage')}</p>`,
                confirmCallback: () => {
                    location.reload()
                },
                onEscape: false,
                closeButton: false,
                showCancelButton:false,
                confirmButtonText: this.$t('WebInterviewUI.Reload'),
            })
        },
    },
}
</script>
