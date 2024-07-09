<template>
    <div class="ag-input-text-wrapper" :id="hash">
        <component ref='editQuestionComponent' :key="question.identity"
            v-bind:is="'MatrixRoster_' + question.entityType" v-bind:id="question.identity" :editorParams="params">
        </component>
        <wb-progress :visible="isFetchInProgress" />
    </div>
</template>

<script lang="js">
import { getLocationHash } from '~/shared/helpers'
import { debounce } from 'lodash'

export default {
    name: 'MatrixRoster_QuestionEditor',

    data() {
        return {
            question: null,
            id: '',
        }
    },
    watch: {
        ['$store.getters.scrollState']() {
            this.scroll()
        },
    },
    mounted() {
        this.scroll()
    },
    computed: {
        $me() {
            return this.$store.state.webinterview.entityDetails[this.question.identity]
        },
        isFetchInProgress() {
            const result = this.$store.state.webinterview.fetch.state[this.question.identity]
            return result
        },
        hash() {
            return getLocationHash(this.question.identity)
        },
    },
    methods: {
        getValue() {
            return this.question
        },
        isCancelBeforeStart() {
            if (this.$me.isDisabled || this.$me.isLocked || !this.$me.acceptAnswer)
                return true
            return false
        },
        isCancelAfterEnd() {
            if (this.$refs.editQuestionComponent.isCancelBeforeStart) {
                var isNeedCancel = this.$refs.editQuestionComponent.isCancelBeforeStart()
                if (isNeedCancel) return true
            }

            if (this.$refs.editQuestionComponent.saveAnswer)
                this.$refs.editQuestionComponent.saveAnswer()

            return false
        },
        destroy() {
            if (this.$refs.editQuestionComponent.destroy)
                this.$refs.editQuestionComponent.destroy()
        },
        doScroll: debounce(function () {
            if (this.$store.getters.scrollState == '#' + this.id) {
                window.scroll({ top: this.$parent.$parent.$el.offsetTop, behavior: 'smooth' })
                this.$store.dispatch('resetScroll')
            }
        }, 200),

        scroll() {
            if (this.$store && this.$store.state.route.hash === '#' + this.id) {
                this.doScroll()
            }
        },
    },
    created() {
        this.question = this.params.value
        this.id = this.question.identity
    },
}
</script>
