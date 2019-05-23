<template>
    <div :class="questionStyle" :id='questionId'>
        <popover  :title="validationTitle" :enable="doesExistValidationMessage" trigger="hover-focus" append-to="body">
            <a class="cell-content has-tooltip" type="primary" data-role="trigger">
                {{ answer }}
            </a>
            <template slot="popover">
                <div class="popover-content error-tooltip" v-html="validationMessage"></div>
            </template>
        </popover>

        <wb-progress :visible="isFetchInProgress" :valuenow="valuenow" :valuemax="valuemax" />
    </div>
</template>

<script lang="js">
    export default {
        name: 'TableRoster_ViewAnswer',

        data() {
            return {
                questionId: null
            }
        }, 
        computed: {
            $me() {
                return this.$store.state.webinterview.entityDetails[this.questionId] 
            },
            answer() {
                return this.$me.answer
            },
            questionStyle() {
                return [{
                    'disabled-question' : this.$me.isDisabled,
                    'has-error' : !this.$me.validity.isValid,
                    'has-warnings' : this.$me.validity.warnings.length > 0,
                    'not-applicable' : this.$me.isLocked,
                    'syncing': this.$me.fetching
                }, 'cell-unit']
            },
            isFetchInProgress() {
                return this.$me.fetching
            },
            doesExistValidationMessage() {
                if (this.$me.validity.messages && this.$me.validity.messages.length > 0)
                    return true
                if (this.$me.validity.warnings && this.$me.validity.warnings.length > 0)
                    return true
                return false
            },
            validationTitle() {
                if (this.$me.validity.messages && this.$me.validity.messages.length > 0)
                    return 'Error'
                if (this.$me.validity.warnings && this.$me.validity.warnings.length > 0)
                    return 'Warning'
                return null
            },
            validationMessage() {
                let message = ''
                const validity = this.$me.validity
                for (let index = 0; index < validity.messages.length; index++) {
                    const errorMessage = validity.messages[index];
                    message += errorMessage + '<br />'
                }
                for (let index = 0; index < validity.warnings.length; index++) {
                    const errorMessage = validity.warnings[index];
                    message += errorMessage + '<br />'
                }
                return message;
            },
            valuenow() {
                if (this.$me.fetchState) {
                    return this.$me.fetchState.uploaded
                }
                return 100
            },
            valuemax() {
                if (this.$me.fetchState) {
                    return this.$me.fetchState.total
                }
                return 100
            }
        },
        methods: {

        },
        created() {
            this.questionId = this.params.value.identity;
        }
    }
</script>











