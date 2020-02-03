<template>
    <span class="lock"
        v-if="!$me.acceptAnswers"></span>
</template>

<script>
import { entityPartial } from '~/webinterview/components/mixins'

export default {
    mixins: [entityPartial],
    name: 'wb-remove-answer',
}
</script>