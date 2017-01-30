<template>
    <wb-question :question="$me" questionCssClassName="time-question">
        <div class="question-unit">
            <div class="options-group">
                <div class="form-group">
                    <div class="field" :class="{answered: $me.isAnswered}">
                        <Flatpickr :options="pickerOpts" :value="answer" class="field-to-fill" placeholder="Enter answer" />
                        <wb-remove-answer />
                    </div>
                </div>
            </div>
        </div>
    </wb-question>
</template>
<script lang="ts">
    import { entityDetails } from "components/mixins"
    import VueFlatpickr from "vue-flatpickr"
    import * as moment from "moment"
    import 'vue-flatpickr/theme/flatpickr.min.css'

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
                    return moment(this.$me.answer).format("YYYY-MM-DD")
                }
                return ""
            }
        },
        methods: {
            answerDate(answer) {
                const oldAnswer = moment(this.$me.answer)
                const newAnswer = moment(answer)

                if (!oldAnswer.isSame(newAnswer)) {
                    this.$store.dispatch('answerDateQuestion', { identity: this.$me.id, date: newAnswer })
                }

            }
        },
        components: {
            "Flatpickr": VueFlatpickr
        }
    }
</script>
