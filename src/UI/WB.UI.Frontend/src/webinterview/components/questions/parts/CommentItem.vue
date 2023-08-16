<template>
    <div :class="{'resolved-comment': resolved, 'enumerators-comment': isInterviewersComment }">
        <h6>{{ commentTitle }} <span class="publication-date"
            :title="this.commentedAtDate">({{this.commentedAt}})</span></h6>
        <p :class="{'overloaded': isCollapsed}">{{text}}
            <button v-if="isCollapsed"
                type="button"
                v-on:click="toggle()"
                class="btn btn-link btn-horizontal-hamburger"><span></span></button>
        </p>
    </div>
</template>

<script lang="js">
import { DateFormats } from '~/shared/helpers'
import moment from 'moment'

export default {
    props: {
        userRole: {
            required: true,
            type: String,
        },
        text: {
            required: true,
            type: String,
        },
        isOwnComment: {
            required: true,
            type: Boolean,
        },
        resolved: {
            required: true,
            type: Boolean,
        },
        date: {
            type: String,
        },
        commentOnPreviousAnswer: {
            required: true,
            type: Boolean,
        },
    },
    data() {
        return {
            isCollapsed: this.text.length > 200,
        }
    },
    computed: {
        isInterviewersComment() {
            return this.userRole == 'Interviewer'
        },
        commentTitle() {
            var title = this.commentTitleByRole
            if (this.commentOnPreviousAnswer == true) {
                return `${title} (${this.$t('WebInterviewUI.CommentOnPreviousAnswer')})`
            }

            return title
        },
        commentTitleByRole() {
            if (this.isOwnComment == true) {
                return this.$t('WebInterviewUI.CommentYours')
            }
            if (this.userRole == 'Administrator') {
                return this.$t('WebInterviewUI.CommentAdmin') // "Admin comment"
            }
            if (this.userRole == 'Supervisor') {
                return this.$t('WebInterviewUI.CommentSupervisor') // "Supervisor comment"
            }
            if (this.userRole == 'Interviewer') {
                return this.$t('WebInterviewUI.CommentInterviewer') // "Interviewer comment"
            }
            if (this.userRole == 'Headquarter') {
                return this.$t('WebInterviewUI.CommentHeadquarters') // "Headquarters comment"
            }

            return this.$t('WebInterviewUI.Comment') //'Comment';
        },
        commentedAt() {
            return moment.utc(this.date).fromNow()
        },
        commentedAtDate() {
            return moment.utc(this.date).format(DateFormats.dateTime)
        },
    },
    methods: {
        toggle() {
            this.isCollapsed = !this.isCollapsed
        },
    },
}

</script>
