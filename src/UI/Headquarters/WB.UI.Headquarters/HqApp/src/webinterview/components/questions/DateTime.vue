<template>
    <wb-question :question="$me"
                 :questionCssClassName="$me.isTimestamp ? 'current-time-question' : 'time-question'">
        <div class="question-unit">
            <div class="options-group">
                <div v-if="!$me.isTimestamp"
                     class="form-group">
                    <div class="field" :class="{answered: $me.isAnswered}">
                        <flat-pickr :config="pickerOpts"
                                    :value="answer"
                                    :disabled="!$me.acceptAnswer"
                                    class="field-to-fill"
                                    :placeholder="noAnswerWatermark"
                                    :title="noAnswerWatermark" />
                        <wb-remove-answer/>
                    </div>
                </div>
                <div v-else>
                    <div class="field"
                         :class="{answered: $me.isAnswered}">
                        <div class="block-with-data">{{ answer }}</div>
                        <wb-remove-answer />
                    </div>
                    <div class="action-btn-holder time-question"
                         @click="answerDate">
                        <button type="button"
                                :disabled="!$me.acceptAnswer"
                                class="btn btn-default btn-lg btn-action-questionnaire">
                            {{ noAnswerWatermark }}
                        </button>
                    </div>
                </div>
            <wb-lock />
            </div>            
        </div>
    </wb-question>
</template>
<script lang="js">

    import { entityDetails } from "../mixins"
    import flatPickr from './ui/vue-flatpickr'
    import { DateFormats } from "~/shared/helpers"

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
            noAnswerWatermark() {
                return !this.$me.acceptAnswer && !this.$me.isAnswered ? this.$t('Details.NoAnswer') : 
                    (this.$me.isTimestamp ? this.$t("WebInterviewUI.RecordCurrentTime") : this.$t('WebInterviewUI.EnterDate'))
            },
            answer() {
                if (this.$me && this.$me.answer) {
                    if (this.$me.isTimestamp){
                        return moment(this.$me.answer).format(DateFormats.dateTime)
                    }
                    else {
                        const result = moment(this.$me.answer).format(DateFormats.date)
                        return result;
                    }
                }
                return ""
            }
        },
        methods: {
            answerDate(answer) {
                this.sendAnswer(() => {
                    if(answer) {
                        if (!this.$me.isTimestamp) {
                            if (!moment(this.$me.answer).isSame(answer)) {
                                const dateAnswer = parseUTC(answer)

                                this.$store.dispatch('answerDateQuestion', { identity: this.$me.id, date: dateAnswer })
                            }
                        }
                        else {
                            this.$store.dispatch('answerDateQuestion', { 
                                identity: this.$me.id,
                                date: moment().format(DateFormats.dateTime)
                            });
                        }
                    }
                });
            }
        },
        components: {
            flatPickr
        }
    }

</script>
