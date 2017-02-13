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
    import 'vue-flatpickr/theme/flatpickr.min.css'
    import * as format from "date-fns/format"
    import * as isSame from "date-fns/is_equal"

    const parseUTC = date => new Date(Date.UTC(date.getFullYear(), date.getMonth(), date.getDate(), date.getHours(), date.getMinutes(), date.getSeconds(), date.getMilliseconds()));

    export default {
        name: "DateTime",
        mixins: [entityDetails],
        data() {
            return {
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
                if (this.$me && this.$me.answer) {
                    const result = format(this.$me.answer, this.$me.isTimestamp ? "YYYY-MM-DD HH:mm:ss" : "YYYY-MM-DD")
                    return result
                }
                return ""
            }
        },
        methods: {
            answerDate(answer: string) {
                if (!this.$me.isTimestamp) {
                    if (!isSame(this.$me.answer, answer)) {
                        const dateAnswer = parseUTC(answer)

                        this.$store.dispatch('answerDateQuestion', { identity: this.$me.id, date: dateAnswer })
                    }
                }
                else {
                    this.$store.dispatch('answerDateQuestion', { identity: this.$me.id, date: new Date() })
                }
            }
        },
        components: {
            "Flatpickr": VueFlatpickr
        }
    }

</script>
