<template>
    <wb-question :question="$me" questionCssClassName="numeric-question">
        <div class="question-unit">
            <div class="options-group">
                <div class="form-group">
                    <div class="field" :class="{ answered: $me.isAnswered }"> 
                        <input type="text" autocomplete="off" inputmode="numeric" class="field-to-fill"
                            :placeholder="noAnswerWatermark" 
                            :title="noAnswerWatermark"
                            :value="$me.answer" v-blurOnEnterKey @blur="answerDoubleQuestion"
                            :disabled="isSpecialValueSelected || !$me.acceptAnswer"
                            :class="{ 'special-value-selected': isSpecialValueSelected }"
                            v-numericFormatting="{aSep: groupSeparator, aDec:decimalSeparator, mDec: $me.countOfDecimalPlaces, vMin: '-99999999999999.99999999999999', vMax: '99999999999999.99999999999999', aPad: false }">
                            <wb-remove-answer v-if="!isSpecialValueSelected" />
                    </div>
                </div>
                <div class="radio" v-if="isSpecialValueSelected != false" v-for="option in $me.options" :key="$me.id + '_' + option.value">
                    <div class="field">
                        <input class="wb-radio" 
                            type="radio" 
                            :id="$me.id + '_' + option.value" 
                            :name="$me.id" 
                            :value="option.value" 
                            :disabled="!$me.acceptAnswer"
                            v-model="specialValue">
                        <label :for="$me.id + '_' + option.value">
                            <span class="tick"></span> {{option.title}}
                        </label>
                        <wb-remove-answer />
                    </div>
                </div>
                <wb-lock />
            </div>
        </div>
    </wb-question>
</template>
<script lang="js">
    import { entityDetails } from "../mixins"
    import * as $ from "jquery"
    export default {
        name: 'Double',
        mixins: [entityDetails],
        computed: {
            isSpecialValueSelected(){
                if (this.$me.answer == null || this.$me.answer == undefined)
                    return undefined;
                return this.isSpecialValue(this.$me.answer);
            },
            noAnswerWatermark() {
                return !this.$me.acceptAnswer && !this.$me.isAnswered ? this.$t('Details.NoAnswer') : this.$t('WebInterviewUI.DecimalEnter')
            },
            groupSeparator() {
                if (this.$me.useFormatting) {                    
                    var etalon = 1111
                    var localizedNumber = etalon.toLocaleString()
                    return localizedNumber.substring(1, localizedNumber.length - 3)
                }

                return ''
            },
            decimalSeparator() {
                if (this.$me.useFormatting) {
                    
                    var etalon = 1.111
                    var localizedNumber = etalon.toLocaleString()
                    return localizedNumber.substring(1, localizedNumber.length - 3)
                }

                return '.'
            },
            specialValue: {
                get() {
                    return this.$me.answer
                },
                set(value) {
                    this.saveAnswer(value, true);
                }
            }
        },
        methods: {
            answerDoubleQuestion(evnt) {
                const answerString = $(evnt.target).autoNumeric('get');
                if (answerString.replace(/[^0-9]/g, "").length > 15) {
                    this.markAnswerAsNotSavedWithMessage(this.$t("WebInterviewUI.DecimalTooBig"))
                    return
                }

                const answer = answerString != undefined && answerString != ''
                    ? parseFloat(answerString)
                    : null;

                const isSpecialValue = this.isSpecialValue(answer);
                this.saveAnswer(answer, isSpecialValue);
            },
            saveAnswer(answer, isSpecialValue){
                this.sendAnswer(() => {
                    if(this.handleEmptyAnswer(answer)) {
                        return
                    }
                    if (answer > 999999999999999 || answer < -999999999999999) {
                        this.markAnswerAsNotSavedWithMessage($t("WebInterviewUI.DecimalCannotParse"))
                        return
                    }

                    this.$store.dispatch('answerDoubleQuestion', { identity: this.id, answer: answer })
                });
            },
            isSpecialValue(value){
                const options = this.$me.options || [];
                if (options.length == 0)
                    return false;
                for(let i=0;i<options.length;i++)
                {
                    if (options[i].value === value)
                        return true;
                }
                return false;
            }
        }
    }

</script>
