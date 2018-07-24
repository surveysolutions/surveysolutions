<template>
    <div v-if="$me.instructions && !$me.isDisabled" class="instructions-wrapper">
        <div class="information-block instruction" v-if="!$me.hideInstructions || shown">
            <h6>{{ $t("WebInterviewUI.Instruction") }}</h6>
            <p v-dateTimeFormatting v-html="$me.instructions"></p>
        </div>
        <button v-else type="button" class="btn btn-link" @click="show">{{ $t("WebInterviewUI.InstructionShow") }}</button>
    </div>
</template>
<script lang="js">
    import { entityPartial } from "~/webinterview/components/mixins"

    export default {
        name: "wb-instructions",
        mixins: [entityPartial],
        data: () => {
            return {
                shown: false,
            }
        },
        methods: {
            show: function () {
                this.shown = true
            }
        }
    }
</script>
