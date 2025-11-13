import { getLocationHash } from '~/shared/helpers'

const designerMixin = {
    methods: {
        openDesigner() {
            if (!this.$config.inWebTesterMode) {
                return
            }

            const { questionnaireId, designerUrl } = this.$config
            const entityId = this.$me.id.split('_')[0]
            const url = `${designerUrl}/q/details/${questionnaireId}/entity/${entityId}`

            window.open(url, '_blank')
        },
    },
}

// Validation, Title, RemoveAnswer, Instruction, Attachment, etc...
export const entityPartial = {
    mixins: [designerMixin],
    computed: {
        $me() {
            const id = this.id || this.$parent.$parent.id

            return this.$store.state.webinterview.entityDetails[id] || {
                isAnswered: false,
                validity: {
                    isValid: true,
                },
                isLoading: true,
            }
        },
    },
    props: ['id']
}

// Questions
export const entityDetails = {
    mixins: [designerMixin],
    computed: {
        $me() {
            let result = null

            if (this.id != null) {
                result = this.$store.state.webinterview.entityDetails[this.id]
            }

            return result || {
                isAnswered: false,
                validity: {
                    isValid: true,
                },
                isLoading: true,
            }
        },

        hash() {
            return getLocationHash(this.id)
        },
        interviewId() {
            return this.$route.params.interviewId
        },
        inFetchState() {
            const fetchState = this.$store.state.webinterview.fetch.state[this.id]
            return fetchState != null && fetchState == true
        },

        acceptAnswer() {
            return this.$me.acceptAnswer
        },
    },

    props:
    {
        id: {
            type: String,
            required: true,
        },

        fetchOnMount: {
            type: Boolean,
            default: false,
        },
    },

    mounted() {
        if (this.fetchOnMount) {
            this.fetch()
        }
    },

    watch: {
        id(to, from) {
            this.$store.dispatch('cleanUpEntity', from)
        },
    },

    destroyed() {
        //this.$store.dispatch("cleanUpEntity", this.id)
    },

    methods: {
        sendAnswer(callback) {
            if (this.acceptAnswer) {
                callback()
                this.$store.dispatch('tryResolveFetch', this.id)
            }
        },

        cleanValidity() {
            this.$store.dispatch('clearAnswerValidity', { id: this.id })
        },
        markAnswerAsNotSavedWithMessage(message, notSavedAnswerValue) {
            this.$store.dispatch('setAnswerAsNotSaved', { id: this.id, message, notSavedAnswerValue })
        },
        removeAnswer() {
            this.$store.dispatch('removeAnswer', this.$me.id)
            this.$emit('answerRemoved', this.$me.id)

        },

        fetch(id) {
            this.$store.dispatch({
                type: 'fetchEntity',
                id: id || this.id,
                source: 'client',
            })
        },

        handleEmptyAnswer(answer) {
            const newAnswer = answer === undefined || answer === null || answer === '' ? null : answer
            const currentAnswer = this.$me.answer === undefined || this.$me.answer === null || this.$me.answer === '' ? null : this.$me.answer

            if (newAnswer === currentAnswer) {
                return true
            }

            if (newAnswer === null && this.$me.isAnswered) {
                this.removeAnswer()
                return true
            }

            return false
        },
    },
}

// Table Roster cell parameters
export const tableCellEditor = {
    props: {
        editorParams: {
            type: Object,
            default: {},
        },
    },
}
