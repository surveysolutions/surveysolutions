<template>
    <wb-question :question="$me" :questionCssClassName="$me.isTimestamp ? 'current-time-question' : 'time-question'">
        <div class="question-unit">
            <div class="options-group">
                <div v-if="!$me.isTimestamp" class="form-group">
                    <div class="field" :class="{answered: $me.isAnswered}">
                        <Flatpickr :options="pickerOpts" :value="answer" class="field-to-fill" placeholder="Enter answer" />
                        <wb-remove-answer/>
                    </div>
                </div>
                <div v-else>
                    <div class="field" :class="{answered: $me.isAnswered}">
                        <div class="block-with-data">{{answer}}</div>
                        <wb-remove-answer />
                    </div>
                    <div class="action-btn-holder time-question" @click="answerDate">
                        <button type="button" class="btn btn-default btn-lg btn-action-questionnaire">
                            Record current time
                        </button>
                    </div>
                    <div>
                    </div>
                </div>
    </wb-question>
</template>
<script lang="ts">
    import { entityDetails } from "components/mixins"
    import VueFlatpickr from "vue-flatpickr"
    import momentPromise from "../../misc/moment"
    import 'vue-flatpickr/theme/flatpickr.min.css'

    let moment = null

    export default {
        name: "DateTime",
        mixins: [entityDetails],
        data() {

            momentPromise.then(value => {
                moment = value
                this.momentLoading = false
            })

            return {
                momentLoading: true,
                pickerOpts: {
                    dateFormat: "Y-m-d",
                    onChange: (selectedDate) => {
                        this.answerDate(selectedDate[0])
                    }
                }
            }
        },
        computed: {
             answer() {
                if (!this.momentLoading && this.$me && this.$me.answer) {
                    const result = moment(this.$me.answer).format(this.$me.isTimestamp ? "YYYY-MM-DD HH:mm:ss" : "YYYY-MM-DD")
                    return result
                }
                return ""
            }
        },
        methods: {
            async answerDate(answer: string) {
                const _moment = await momentPromise
                if (!this.$me.isTimestamp) {
                    const oldAnswer =  _moment(this.$me.answer)
                    const typedAnswer =  _moment(answer)
                    const newAnswer =  _moment.utc([typedAnswer.year(), typedAnswer.month(), typedAnswer.date()])

                    if (!oldAnswer.isSame(answer)) {
                        this.$store.dispatch('answerDateQuestion', { identity: this.$me.id, date: newAnswer })
                    }
                }
                else {
                    this.$store.dispatch('answerDateQuestion', { identity: this.$me.id, date:  _moment() })
                }
            }
        },
        components: {
            "Flatpickr": VueFlatpickr
        }
    }

</script>
