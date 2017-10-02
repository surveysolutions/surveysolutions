<template>
    <wb-question :question="$me" :questionCssClassName="$me.isTimestamp ? 'current-time-question' : 'time-question'">
        <div class="question-unit">
            <div class="options-group">
                <div v-if="!$me.isTimestamp" class="form-group">
                    <div class="field" :class="{answered: $me.isAnswered}">
                        <flat-pickr :config="pickerOpts" :value="answer" class="field-to-fill"
                            :placeholder="$t('EnterDate')" :title="$t('EnterDate')" />
                        <wb-remove-answer/>
                    </div>
                </div>
                <div v-else>
                    <div class="field" :class="{answered: $me.isAnswered}">
                        <div class="block-with-data">{{ answer }}</div>
                        <wb-remove-answer />
                    </div>
                    <div class="action-btn-holder time-question" @click="answerDate">
                        <button type="button" class="btn btn-default btn-lg btn-action-questionnaire">
                            {{ $t("RecordCurrentTime") }}
                        </button>
                    </div>
                </div>
            </div>
        </div>
    </wb-question>
</template>
<script lang="js">
    import { entityDetails } from "../mixins"
    import flatPickr from './ui/vue-flatpickr'
    // import 'flatpickr/dist/flatpickr.css'
    import * as format from "date-fns/format"
    import * as isSame from "date-fns/is_equal"
    import { DateFormats } from "../questions"

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
                    if (this.$me.isTimestamp){
                        return format(this.$me.answer, DateFormats.dateTime)
                    }
                    else {
                        const date = new Date(this.$me.answer)
                        const result = format(new Date(date.getUTCFullYear(), date.getUTCMonth(), date.getUTCDate()), DateFormats.date)
                        return result;
                    }
                }
                return ""
            }
        },
        methods: {
            answerDate(answer) {
                if(answer) {
                    if (!this.$me.isTimestamp) {
                        if (!isSame(this.$me.answer, answer)) {
                            const dateAnswer = parseUTC(answer)

                            this.$store.dispatch('answerDateQuestion', { identity: this.$me.id, date: dateAnswer })
                        }
                    }
                    else {
                        this.$store.dispatch('answerDateQuestion', { identity: this.$me.id, date: new Date().toLocaleString() })
                    }
                }
            }
        },
        components: {
            flatPickr
        }
    }

</script>
