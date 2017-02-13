<template>
</template>
<script lang="ts">
    import * as bootbox from "bootbox"
    import * as diffInMinutes from "date-fns/difference_in_minutes"

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
        created() {
            setInterval(() => {
                if (!this.shown) {
                    const minutesAfterLastAction = diffInMinutes(new Date(), this.$store.state.lastActivityTimestamp)
                    if (Math.abs(minutesAfterLastAction) > 15) {
                        this.show()
                    }
                }
            }, 15 * 1000)
        }
    }

</script>
