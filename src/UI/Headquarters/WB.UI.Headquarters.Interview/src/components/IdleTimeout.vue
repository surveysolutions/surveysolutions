<template>
</template>
<script lang="ts">
    import * as bootbox from "bootbox"
    import moment from "../misc/moment"

    export default {
        data: () => {
            return {
                shown: false
            }
        },
        methods: {
            show() {
                if (!this.shown) {
                    this.shown = true
                    this.$store.dispatch("stop")
                    bootbox.alert({
                        title: "Your session has timed out",
                        message: "<p>Your session has timed out because you didn't do any action for 15 minutes.</p><p>Please reload the page to continue this interview.</p>",
                        callback: () => {
                            location.reload()
                        },
                        onEscape: false,
                        closeButton: false,
                        buttons: {
                            ok: {
                                label: "Reload",
                                className: "btn-success"
                            }
                        }
                    })
                }
            }
        },
        async created() {
            const momentInstance = await moment

            setInterval(() => {
                if (!this.shown) {
                    const minutesAfterLastAction = momentInstance().diff(this.$store.state.lastActivityTimestamp, "minutes")
                    if (Math.abs(minutesAfterLastAction) > 15) {
                        this.show()
                    }
                }
            }, 15 * 1000)
        }
    }

</script>
