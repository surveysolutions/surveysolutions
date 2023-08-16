<template>
    <div v-if="$me.instructions && !$me.isDisabled"
        class="instructions-wrapper">
        <div class="information-block instruction"
            v-if="!$me.hideInstructions || shown">
            <div v-dateTimeFormatting
                v-linkToRoute
                v-html="$me.instructions"></div>
        </div>
        <button v-else
            type="button"
            class="btn btn-link"
            @click="show">
            {{ $t("WebInterviewUI.InstructionShow") }}
        </button>
    </div>
</template>
<script lang="js">
import { entityPartial } from '~/webinterview/components/mixins'

export default {
    name: 'wb-instructions',
    mixins: [entityPartial],
    data: () => {
        return {
            shown: false,
        }
    },
    methods: {
        show: function () {
            this.shown = true
        },
    },
}
</script>
