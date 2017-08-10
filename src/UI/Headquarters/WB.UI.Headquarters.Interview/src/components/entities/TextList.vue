<template>
    <wb-question :question="$me" questionCssClassName="single-select-question">
        <div class="question-unit">
            <div class="options-group">
                <div class="form-group" v-for="(row, index) in $me.rows">
                    <div class="field answered">
                        <input autocomplete="off" type="text" class="field-to-fill" :value="row.text" v-blurOnEnterKey @blur="updateRow($event, row)"/>
                        <button type="submit" class="btn btn-link btn-clear" v-on:click="confirmAndRemoveRow(index)"><span></span></button>
                    </div>
                </div>
                <div class="form-group" v-if="canAddNewItem">
                    <div class="field answered">
                        <input autocomplete="off" type="text" class="field-to-fill"
                            :placeholder="$t('TextEnterNewItem')" :title="$t('TextEnterNewItem')" v-blurOnEnterKey @blur="addRow"/>
                    </div>
                </div>
            </div>
        </div>
    </wb-question>
</template>
<script lang="js">
    import { entityDetails } from "components/mixins"
    import * as $ from "jquery"
    import modal from "../../modal"

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
            canAddNewItem(){
                return this.$me.rows == undefined || this.$me.maxAnswersCount == null || this.$me.maxAnswersCount > this.$me.rows.length
            }
        },
        methods: {
            confirmAndRemoveRow(index) {
                if (!this.$me.isRosterSize) {
                    this.removeRow(index)
                    return
                }

                modal.confirm(this.$t('ConfirmRosterRemove'), result => {
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
                this.$me.rows.splice(index, 1)
                if (this.$me.rows.length == 0)
                    this.$store.dispatch('removeAnswer', this.id)
                else
                    this.$store.dispatch('answerTextListQuestion', { identity: this.id, rows: this.$me.rows })
            },
            updateRow(evnt, item) {
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
