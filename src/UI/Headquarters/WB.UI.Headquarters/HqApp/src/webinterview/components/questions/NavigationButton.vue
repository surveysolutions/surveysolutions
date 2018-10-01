<template>
    <div class="action-container" v-if="$me && visible">
        <a class="btn btn-lg" :class="css" @click="navigate">
            <span v-html="buttonTitle"></span>
        </a>
    </div>
</template>
<script lang="js">
    import { entityDetails } from "../mixins"
    import { GroupStatus, ButtonType } from "../questions"

    export default {
        mixins: [entityDetails],
        name: "NavigationButton",
        computed: {
            visible(){
                return !(this.$store.getters.isReviewMode === true) || this.$me.type != ButtonType.Complete
            },
            css() {
                const status =  this.$me.status
                const isValid = this.$me.validity.isValid

                return [{
                    'btn-success': isValid && status == GroupStatus.Completed,
                    'btn-danger': isValid == false || status == GroupStatus.Invalid || status == GroupStatus.StartedInvalid || status == GroupStatus.CompletedInvalid,
                    'btn-primary': isValid && (status == GroupStatus.NotStarted || status == GroupStatus.Started), 
                    'btn-back': this.isParentButton
                }]
            },
            to() {
                return {
                    name: 'section',
                    params: {
                        sectionId: this.$me.target,
                        interviewId: this.$route.params.interviewId
                    },
                    hash: this.isParentButton ? '#' + this.$route.params.sectionId : ''
                }
            },
            isParentButton() {
                return this.$me.type == ButtonType.Parent
            },
            buttonTitle() {
                if(this.$me == null || this.$me.title == null)
                    return "";

                var title = this.$me.title;

                if(this.$me.rosterTitle != null)
                    title +=" - <i>" +  this.$me.rosterTitle + "</i>";

                return title;
            }
        },

        watch: {
            ["$route.params.sectionId"]() {
                this.fetch();
            }
        },

        methods: {
            navigate() {
                if (this.$me.type == ButtonType.Complete) {
                    this.$router.push({ name: "complete" })
                }
                else {
                    if (this.isParentButton) {
                        this.$store.dispatch("sectionRequireScroll", { id: this.$route.params.sectionId })
                    }
                    this.$router.push(this.to)
                }
            }
        }
    }

</script>
