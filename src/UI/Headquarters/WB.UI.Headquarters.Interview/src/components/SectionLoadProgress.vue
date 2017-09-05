<template>
    <div v-if="visible" class="loading">
        <div>{{ $t("LoadingWait") }}</div>
    </div>
</template>
<script lang="js">
    import * as delay from "lodash/delay"

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
            '$store.state.connection.isDisconnected' (to, from) {
                if (to) {
                    this.visible = false
                }
            }
        },
        computed: {
            isLoading() {
                const loadedCount = this.$store.state.loadedEntitiesCount
                const totalCount = this.$store.state.entities.length

                return loadedCount === 0 || totalCount === 0 || (loadedCount < totalCount)
            }
        }
    }

</script>
