<template>
    <component :is="componentName"></component>
</template>

<script lang="ts">
    export default {
        name: "cover-view",
        computed: {
            componentName() {
                return this.$store.state.isSampleMode ? "CoverReadonly": "Section"
            }
        }
    }
</script>
