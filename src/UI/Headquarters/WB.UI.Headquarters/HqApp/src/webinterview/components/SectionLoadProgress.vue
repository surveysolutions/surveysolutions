<template>
    <div v-if="visible" class="loading">
        <div>{{ $t("WebInterviewUI.LoadingWait") }}</div>
    </div>
</template>
<script lang="js">
    import { delay } from "lodash"

    export default {
        data() {
            return {
                visible: false,
                timerId: null,
                delay: 100
            }
        },
        watch: {
            isLoading(to, from) {
                if (from === false) {
                    this.timerId = delay(() => this.visible = to, this.delay)
                } else {
                    if (this.timerId != null) {
                        clearTimeout(this.timerId)
                        this.timerId = null
                    }

                    this.visible = this.to
                }
            },
            '$store.state.webinterview.connection.isDisconnected' (to) {
                if (to) {
                    this.visible = false
                }
            }
        },
        computed: {
            isLoading() {
                return this.$store.getters.loadingProgress;
            }
        }
    }

</script>
