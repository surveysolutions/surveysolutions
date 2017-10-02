<template>
    <h5 v-dateTimeFormatting v-html="$me.title"></h5>
</template>
<script lang="js">
    import { entityPartial } from "shared/components/mixins"

    export default {
        mixins: [entityPartial],
        name: "wb-title"
    }
</script>
