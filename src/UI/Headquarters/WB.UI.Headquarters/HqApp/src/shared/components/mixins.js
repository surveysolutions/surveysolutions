import { getLocationHash } from "../helpers"

// Validation, Title, RemoveAnswer, Instruction, Attachment
export const entityPartial = {
    computed: {
        $me() {
            const id = this.id || this.$parent.id

            return this.$store.state.entityDetails[id] || {
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

export function detailsMixin(fetchMethod, defaults) {
    return {
        computed: {
            $me() {
                let result = null

                if (this.id != null) {
                    result = this.$store.state.entityDetails[this.id]
                }

                return result || defaults
            },
            hash() {
                return getLocationHash(this.id)
            },
            interviewId() {
                return this.$route.params.interviewId
            }
        },
        props: ["id"],
        mounted() {
            this.fetch()
        },
        watch: {
            id(to, from) {
                this.$store.dispatch("cleanUpEntity", from)
                this.fetch(to)
            }
        },
        destroyed() {
            this.$store.dispatch("cleanUpEntity", this.id)
        },
        methods: {
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
                    type: fetchMethod,
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
}

// Questions
export const entityDetails = detailsMixin("fetchEntity", {
    isAnswered: false,
    validity: {
        isValid: true
    },
    isLoading: true
})
