<template>
    <div v-if="show">
        <p class="text-warning">Loading...</p>
    </div>
</template>
<script lang="ts">
    import * as toastr from "toastr"
    export default {
        computed: {
            show() {
                const loadedCount = this.$store.state.loadedEntitiesCount
                const totalCount = this.$store.state.entities.length

                return false /*temp disable this before we have markup*/ || loadedCount === 0 || totalCount === 0 || (loadedCount < totalCount)
            }
        }
    }

</script>
