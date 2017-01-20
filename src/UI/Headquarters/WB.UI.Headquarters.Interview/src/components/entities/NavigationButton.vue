<template>
    <div class="action-container" v-if="$me">
        <a class="btn btn-lg" :class="[{'btn-back': icon}, css]" @click="navigate"> {{ $me.title}}</a>        
    </div>
</template>
<script lang="ts">
    import { entityDetails } from "components/mixins"
    import { GroupStatus, ButtonType } from "components/entities"
    import * as _ from "lodash"

    export default {
        mixins: [entityDetails],
        name: "NavigationButton",
        computed: {
            icon() {
                if (this.isParentButton) {
                    return true
                } else {
                    return null
                }
            },
            css() {
                return [{
                    'btn-success': this.$me.status == GroupStatus.Completed,
                    'btn-danger': this.$me.status == GroupStatus.Invalid,
                    'btn-default': this.isParentButton,
                    'btn-primary': !this.isParentButton
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
            $route(to, fro,) {
                this.fetch();
            }
        },
        methods: {
            navigate() {
                if (this.isParentButton) {
                    this.$store.dispatch("fetch/sectionRequireScroll", { id: this.$route.params.sectionId })
                }
                this.$router.push(this.to)
            }
        }
    }
</script>
<style>
.rotate-270{
    transform: rotate(270deg);
}
</style>
