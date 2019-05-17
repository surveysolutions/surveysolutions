<template>
    <div :class="questionStyle" :id='questionId'>
        <a class="cell-content has-tooltip" template="" tabindex="0" role="button" data-placement="auto top" data-toggle="popover" data-trigger="focus" title="Dismissible popover" :data-content="validationMessage" style="display-block">
            {{ answer }}
        </a>
        <div class="">

        </div>
        <wb-progress :visible="isFetchInProgress" :valuenow="valuenow" :valuemax="valuemax" />
        <!--div class="progress">
            <div class="progress-bar" role="progressbar" :visible="isFetchInProgress" :valuenow="valuenow" :valuemax="valuemax">
                <span class="sr-only">60% Complete</span>
            </div>
        </div-->
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
            validationMessage() {
                var message = ''
                var validity = this.$me.validity
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











