import { getLocationHash } from "~/shared/helpers"

// Validation, Title, RemoveAnswer, Instruction, Attachment, etc...
export const entityPartial = {
    computed: {
        $me() {
            const id = this.id || this.$parent.id

            return this.$store.state.webinterview.entityDetails[id] || {
                isAnswered: false,
                validity: {
                    isValid: true
                },
                isLoading: true
            }
        }
    },
    props: ["id"]
}

// Questions
export const entityDetails = {
    computed: {
        $me() {
            let result = null

            if (this.id != null) {
                result = this.$store.state.webinterview.entityDetails[this.id]
            }

            return result || {
                isAnswered: false,
                validity: {
                    isValid: true
                },
                isLoading: true
            }
        },
        hash() {
            return getLocationHash(this.id)
        },
        interviewId() {
            return this.$route.params.interviewId
        },
        inFetchState() {
            return this.$me.fetchState != null && this.$me.fetching == true;
        },

        acceptAnswer() {
            return this.$me.acceptAnswer;
        }
    },

    props:
    {
        id: {
            type: String,
            required: true
        },

        fetchOnMount: {
            type: Boolean,
            default: false
        }
    },

    mounted() {
        if(this.fetchOnMount){
            this.fetch()
        }
    },

    watch: {
        id(to, from) {
            this.$store.dispatch("cleanUpEntity", from)
        }
    },

    destroyed() {
        this.$store.dispatch("cleanUpEntity", this.id)
    },

    methods: {
        sendAnswer(callback) {
            if (this.acceptAnswer) {
                callback();
            }
        },
        cleanValidity() {
            this.$store.dispatch("clearAnswerValidity", { id: this.id })
        },
        markAnswerAsNotSavedWithMessage(message) {
            this.$store.dispatch("setAnswerAsNotSaved", { id: this.id, message })
        },
        removeAnswer() {
            this.$store.dispatch("removeAnswer", this.$me.id)
            this.$emit("answerRemoved", this.$me.id)
        },

        fetch(id) {
            this.$store.dispatch({
                type: "fetchEntity",
                id: id || this.id,
                source: "client"
            })
        },

        handleEmptyAnswer(answer) {
            const answ = answer === undefined || answer === null || answer === "" ? null : answer

            if (answ === this.$me.answer) {
                return true
            }

            if ((answ === "" || answ === null) && this.$me.isAnswered) {
                this.removeAnswer()
                return true
            }

            return false
        }
    }
}

