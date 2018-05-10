<template>
    <wb-question :question="$me" questionCssClassName="single-select-question">
        <div class="question-unit">
            <div class="options-group">
                <div class="form-group" v-for="(row, index) in $me.rows" :key="row.value">
                    <div class="field answered"   v-bind:class="{ 'unavailable-option locked-option': row.isProtected }">
                        <input autocomplete="off" type="text" class="field-to-fill" 
                            :value="row.text"
                            :disabled="!$me.acceptAnswer || row.isProtected"
                            v-blurOnEnterKey 
                            @blur="updateRow($event, row)"/>
                        <button type="submit" class="btn btn-link btn-clear" 
                            v-if="$me.acceptAnswer && !row.isProtected"
                            tabindex="-1"
                            @click="confirmAndRemoveRow(index)"><span></span></button>
                    </div>
                </div>
                <div class="form-group" v-if="canAddNewItem">
                    <div class="field answered">
                        <input autocomplete="off" type="text" class="field-to-fill"
                            :disabled="!canAnswer"
                            :placeholder="noAnswerWatermark" v-blurOnEnterKey @blur="addRow"/>
                    </div>
                </div>
                <wb-lock />
            </div>
        </div>
    </wb-question>
</template>
<script lang="js">
    import { entityDetails } from "../mixins"
    import modal from "../modal"

    class TextListAnswerRow {
       constructor(value, text) {
            this.value = value
            this.text = text
        }
    }

    export default {
        name: 'TextList',
        mixins: [entityDetails],
        computed:{
            canAddNewItem() {
                if(this.$store.getters.isReviewMode) {
                    return !this.$me.isAnswered;
                }

                return this.$me.rows == undefined || this.$me.maxAnswersCount == null || this.$me.maxAnswersCount > this.$me.rows.length
            },
            canAnswer() {
                return this.$me.acceptAnswer;
            },
            noAnswerWatermark() {
                return !this.$me.acceptAnswer && !this.$me.isAnswered ? this.$t('Details.NoAnswer') : this.$t('WebInterviewUI.TextEnterNewItem')
            }
        },
        methods: {
            confirmAndRemoveRow(index) {
                if(!this.canAnswer) return;

                if (!this.$me.isRosterSize) {
                    this.removeRow(index)
                    return
                }

                modal.confirm(this.$t('WebInterviewUI.ConfirmRosterRemove'), result => {
                    if (result) {
                        this.removeRow(index)
                        return;
                    } else {
                        this.fetch()
                        return
                    }
                })
            },

            removeRow(index) {
                if(!this.canAnswer) return;

                this.$me.rows.splice(index, 1)
                if (this.$me.rows.length == 0)
                    this.$store.dispatch('removeAnswer', this.id)
                else
                    this.$store.dispatch('answerTextListQuestion', { identity: this.id, rows: this.$me.rows })
            },

            updateRow(evnt, item) {
                if(!this.canAnswer) return;

                const target = $(evnt.target)
                let text = target.val()

                if (item.text == text) return

                if (!text || !text.trim() || text.trim() === item.text) {
                    return
                }
                item.text = text;
                this.$store.dispatch('answerTextListQuestion', { identity: this.id, rows: this.$me.rows })
            },
            addRow(evnt) {
                if(!this.canAnswer) return;

                const target = $(evnt.target)
                let text = target.val()

                if (!text || !text.trim()) return

                let newRowValue = 1
                if (this.$me.rows != undefined && this.$me.rows.length > 0)
                    newRowValue = this.$me.rows[this.$me.rows.length - 1].value + 1

                this.$me.rows.push(new TextListAnswerRow(newRowValue, text))

                this.$store.dispatch('answerTextListQuestion', { identity: this.id, rows: this.$me.rows })
                target.val(undefined)
                target.focus()
            }
        }
    }
</script>
