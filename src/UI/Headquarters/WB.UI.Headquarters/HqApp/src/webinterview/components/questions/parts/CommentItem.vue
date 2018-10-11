<template>
    <div :class="{'enumerators-comment': isInterviewersComment}">
        <h6>{{ commentTitle }}</h6>
        <p :class="{'overloaded': isCollapsed}">{{ text }}<button v-if="isCollapsed" type="button" v-on:click="toggle()" class="btn btn-link btn-horizontal-hamburger"><span></span></button></p>
    </div>
</template>

<script lang="js">
    export default {
        props: {
            userRole: {
                required: true,
                type: Number
            },
            text: {
                required: true,
                type: String
            },
            isOwnComment: {
                required: true,
                type: Boolean
            },
        },
        data() {
            return {
                isCollapsed: this.text.length > 200
            };
        },
        computed: {
            isInterviewersComment(){
                return this.userRole == 4 /*'Interviewer'*/;
            },
            commentTitle() {
                if (this.isOwnComment == true) {
                    return this.$t("WebInterviewUI.CommentYours")
                }
                if (this.userRole == 1 /*'Administrator'*/) {
                    return this.$t("WebInterviewUI.CommentAdmin") // "Admin comment"
                }
                if (this.userRole == 2/*'Supervisor'*/) {
                    return this.$t("WebInterviewUI.CommentSupervisor") // "Supervisor comment"
                }
                if (this.userRole == 4/*'Interviewer'*/) {
                    return this.$t("WebInterviewUI.CommentInterviewer") // "Interviewer comment"
                }
                if (this.userRole == 6/*'Headquarter'*/) {
                    return this.$t("WebInterviewUI.CommentHeadquarters") // "Headquarters comment"
                }

                return this.$t("WebInterviewUI.Comment") //'Comment';
            }           
        },
        methods: {
            toggle(){
                this.isCollapsed = !this.isCollapsed;
            }
        }
    }

</script>
