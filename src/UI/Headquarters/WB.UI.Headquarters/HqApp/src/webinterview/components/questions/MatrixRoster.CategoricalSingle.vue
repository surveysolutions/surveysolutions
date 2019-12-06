<template>
    <div style="display:flex;" class="options-group" v-bind:class="{ 'dotted': noOptions }" v-if="!disabled">
                <div class="radio" v-for="option in answeredOrAllOptions" :key="$me.id + '_' + option.value">
                    <div style="width:220px;border: 1px solid #E5E5E5; margin:3px;" class="field"> 
                        <input style="text-align:center; vertical-align:middle" class="wb-radio" type="radio" 
                          :id="`${$me.id}_${option.value}`" 
                          :name="$me.id" 
                          :value="option.value" 
                          :disabled="!$me.acceptAnswer" 
                          v-model="answer"
                          @change="change">
                        <label :for="$me.id + '_' + option.value">
                            <span class="tick"></span> 
                        </label>
                        <wb-remove-answer :id-suffix="`_opt_${option.value}`"/>
                    </div>
                </div>                
                <div v-if="noOptions" class="options-not-available">{{ $t("WebInterviewUI.OptionsAvailableAfterAnswer") }}</div>
                <wb-lock />
            </div>
</template>

<script lang="js">
    import Vue from 'vue'
    import { entityDetails, tableCellEditor } from "../mixins"
    import { shouldShowAnsweredOptionsOnlyForSingle } from "./question_helpers"

    export default {
        name: 'MatrixRoster_CategoricalSingle',
        mixins: [entityDetails, tableCellEditor],
        props: ['isDisabled'],
        data() {
            return {
                showAllOptions: false,
                answer: null
            }
        }, 
        computed: {
            shouldShowAnsweredOptionsOnly(){
                return shouldShowAnsweredOptionsOnlyForSingle(this);
            },
            disabled() {
                if (this.$me.isDisabled || this.$me.isLocked || !this.$me.acceptAnswer)
                    return true
                return false
            },
            noOptions() {
                return this.$me.options == null || this.$me.options.length == 0
            },
            answeredOrAllOptions(){
                if(!this.shouldShowAnsweredOptionsOnly)
                    return this.$me.options;
                
                var self = this;
                return [find(this.$me.options, function(o) { return o.value == self.answer; })];
            }            
        },
        methods: {
            change() {
                this.sendAnswer(() => {
                    this.answerSingle(this.answer);
                });
            },
            answerSingle(value) {
                this.$store.dispatch("answerSingleOptionQuestion", { answer: value, identity: this.$me.id })
            },
            questionId()  {
                return this.params.value.identity
            },            
            toggleOptions(){
                this.showAllOptions = !this.showAllOptions;
            }
        },
        mounted() {
            this.answer = this.$me.answer
        }
    }
</script>











