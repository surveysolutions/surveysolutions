<template>
    <button type="button"
        class="btn-link button-flag" 
        @click="setFlag"
        :class="{ flagged: hasFlag }"
        :title="flagBtnTitle"
        :disabled="flagBtnDisabled">
    </button>
</template>

<script lang="js">
import { entityPartial } from '~/webinterview/components/mixins'

export default {
    mixins: [entityPartial],
    name: 'wb-flag',
    computed: {
        flagBtnTitle() {
            if (this.$store.state.webinterview.receivedByInterviewer) {
                return this.$t('WebInterviewUI.InterviewReceivedCantModify')
            }

            if (this.hasFlag) {
                return this.$t('Details.FlagTitleFlagged')
            } else {
                return this.$t('Details.FlagTitleUnflagged')
            }
        },
        flagBtnDisabled() {
            return this.$store.state.webinterview.interviewCannotBeChanged
        },
        hasFlag() {
            return this.$store.getters.flags[this.$me.id] === true
        },
    },
    methods:{
        setFlag(){
            this.$store.dispatch('setFlag', { questionId: this.$me.id, hasFlag: !this.hasFlag})
        },
    },
}
</script>
