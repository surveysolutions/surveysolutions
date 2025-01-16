<template>
    <h5 v-dateTimeFormatting v-linkToRoute v-dompurify-html="$me.title"></h5>
</template>
<script lang="js">
import { entityPartial } from '~/webinterview/components/mixins'

export default {
    mixins: [entityPartial],
    name: 'wb-title',
}
</script>
