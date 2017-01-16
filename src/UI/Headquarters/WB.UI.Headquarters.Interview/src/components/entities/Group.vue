<template>
    <div class="question"
            v-if="!$me.isLoading && !($me.isDisabled && $me.hideIfDisabled)"
            :id="hash"
            :class="[{'hidden-question': $me.isDisabled}]">
        <div class="question-editor">
            <div class="question-unit">
                <div class="options-group">
                    <div class="form-group">
                        <div class="field answered">
                            <router-link :to="navigateTo" class="btn btn-primary btn-block">{{calcTitle}}</router-link>
                        </div>
                    </div>
                </div>
            </div>
        </div>
</template>

<script lang="ts">
    import { entityDetails } from "components/mixins"

    export default {
        name: 'Group',
        mixins: [entityDetails],
        computed: {
            navigateTo() {
                return {
                    name: 'section', params: {
                        sectionId: this.id,
                        interviewId: this.$route.params.interviewId
                    }, query: {
                        questionId: "." + this.id
                    }
                }
            },
            calcTitle() {
                //if (this.$me.rosterTitle == null)
                //{
                    return this.$me.title
                //}
            }
        },
        data: () => {
            return {
                text: ''
            }
        }
    }
</script>
