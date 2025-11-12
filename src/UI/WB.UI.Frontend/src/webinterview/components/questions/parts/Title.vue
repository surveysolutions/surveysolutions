<template>
    <h5>
        <a class="open-disigner" v-if="this.$config.inWebTesterMode && $me.name" href="#" @click="openDesigner"
            v-dompurify-html="'[' + $me.name + ']'"></a>
        <span v-dateTimeFormatting v-linkToRoute v-dompurify-html="$me.title"></span>
    </h5>
</template>
<script lang="js">
import { entityPartial } from '~/webinterview/components/mixins'

export default {
    mixins: [entityPartial],
    name: 'wb-title',
}
</script>
