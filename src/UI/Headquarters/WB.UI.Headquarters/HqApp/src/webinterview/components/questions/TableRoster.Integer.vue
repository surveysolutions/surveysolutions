<template>
    <!--input :ref="'input'" @keydown="onKeyDown($event)" v-model="value"/-->


    <input :ref="'input'" type="text" autocomplete="off" inputmode="numeric" class="ag-cell-edit-input" 
        :value="$me.answer" v-blurOnEnterKey 
        @blur="answerIntegerQuestion" 
        v-numericFormatting="{digitGroupSeparator: groupSeparator, decimalPlaces: 0, minimumValue: '-2147483648', maximumValue: '2147483647'}" />
</template>

<script lang="js">
    import Vue from 'vue'
    import { entityDetails } from "../mixins"
    
    export default {
        name: 'TableRoster_Integer',
        mixins: [entityDetails],
        
        data() {
            return {
                autoNumericElement: null,
                cancelBeforeStart: true
            }
        },
        computed: {
            groupSeparator() {
                if (this.$me.useFormatting) {
                    var etalon = 1111
                    var localizedNumber = etalon.toLocaleString()
                    return localizedNumber.substring(1, localizedNumber.length - 3)
                }
                return ''
            }
        },
        methods: {

            saveAnswer() {
                this.answerIntegerQuestion()
            },

            answerIntegerQuestion() {
                const answerString = this.autoNumericElement.getNumericString();
                const answer = answerString != undefined && answerString != ''
                    ? parseInt(answerString)
                    : null;

                this.saveAnswer(answer);
            },

            saveIntegerAnswer(answer){
                this.sendAnswer(() => {
                    if(this.handleEmptyAnswer(answer)) {
                        return
                    }

                    if (answer > 2147483647 || answer < -2147483648 || answer % 1 !== 0) {
                        this.markAnswerAsNotSavedWithMessage(this.$t("WebInterviewUI.NumberCannotParse"))
                        return
                    }

                    if (this.$me.isProtected && this.$me.protectedAnswer > answer) {
                        this.markAnswerAsNotSavedWithMessage(this.$t("WebInterviewUI.NumberCannotBeLessThanProtected"))
                        return
                    }

                    if (!this.$me.isRosterSize) {
                        this.$store.dispatch('answerIntegerQuestion', { identity: this.id, answer: answer })
                        return
                    }

                    if (answer < 0) {
                        this.markAnswerAsNotSavedWithMessage(this.$t("WebInterviewUI.NumberRosterError", { answer }))
                        return;
                    }

                    if (answer > this.$me.answerMaxValue) {
                        this.markAnswerAsNotSavedWithMessage(this.$t("WebInterviewUI.NumberRosterUpperBound", { answer, answerMaxValue: this.$me.answerMaxValue }))
                        return;
                    }

                    const previousAnswer = this.$me.answer
                    const isNeedRemoveRosters = previousAnswer != undefined && answer < previousAnswer

                    if (!isNeedRemoveRosters) {
                        this.$store.dispatch('answerIntegerQuestion', { identity: this.id, answer: answer })
                        return
                    }

                    const amountOfRostersToRemove = previousAnswer -  Math.max(answer, 0);
                    const confirmMessage = this.$t("WebInterviewUI.NumberRosterRemoveConfirm", { amountOfRostersToRemove })

                    if(amountOfRostersToRemove > 0){
                        modal.confirm(confirmMessage, result => {
                            if (result) {
                                this.$store.dispatch('answerIntegerQuestion', { identity: this.id, answer: answer })                                
                                return
                            } else {
                                this.fetch()
                                return
                            }
                        });
                    }
                    else
                    {
                        this.$store.dispatch('answerIntegerQuestion', { identity: this.id, answer: answer })                        
                        return
                    }
                });
            },

            isCancelBeforeStart() {
                return this.cancelBeforeStart;
            },

            // will reject the number if it greater than 1,000,000
            // not very practical, but demonstrates the method.
            isCancelAfterEnd() {
                return this.value > 1000000;
            },

            onKeyDown(event) {
                if (!this.isKeyPressedNumeric(event)) {
                    if (event.preventDefault) event.preventDefault();
                }
            },

            getCharCodeFromEvent(event) {
                event = event || window.event;
                return (typeof event.which === "undefined") ? event.keyCode : event.which;
            },

            isCharNumeric(charStr) {
                return /\d/.test(charStr);
            },

            isKeyPressedNumeric(event) {
                const charCode = this.getCharCodeFromEvent(event);
                const charStr = String.fromCharCode(charCode);
                return this.isCharNumeric(charStr);
            },
            destroy() {
                if (this.autoNumericElement) {
                    this.autoNumericElement.remove()
                }
            }
        },
        created() {
            // only start edit if key pressed is a number, not a letter
            this.cancelBeforeStart = this.params.charPress && ('1234567890'.indexOf(this.params.charPress) < 0);
        },
        mounted() {
            Vue.nextTick(() => {
                if (this.$refs.input) {
                    this.$refs.input.focus();
                    this.$refs.input.select();
                }
            });
        },
        beforeDestroy () {
            this.destroy()
        }
    }
</script>
