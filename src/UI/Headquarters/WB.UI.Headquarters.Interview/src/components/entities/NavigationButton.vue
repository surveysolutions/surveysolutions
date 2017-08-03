<template>
    <div class="action-container" v-if="$me">
        <a class="btn btn-lg" :class="css" @click="navigate"><span>{{ $me.title}}</span></a>
    </div>
</template>
<script lang="js">
    import { entityDetails } from "components/mixins"
    import { GroupStatus, ButtonType } from "components/entities"

    export default {
        mixins: [entityDetails],
        name: "NavigationButton",
        computed: {
            css() {
                return [{
                    'btn-success': this.$me.status == GroupStatus.Completed,
                    'btn-danger': this.$me.status == GroupStatus.Invalid,
                    'btn-primary': this.$me.status == GroupStatus.Other,
                    'btn-back': this.isParentButton
                }]
            },
            to() {
                return {
                    name: 'section',
                    params: {
                        sectionId: this.$me.target,
                        interviewId: this.$route.params.interviewId
                    }
                }
            },
            isParentButton() {
                return this.$me.type == ButtonType.Parent
            }
        },
        watch: {
            $route(to, fro, ) {
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
