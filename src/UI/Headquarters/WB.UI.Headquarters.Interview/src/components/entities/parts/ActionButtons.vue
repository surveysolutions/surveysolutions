<template>
    <div class="question">
        <div class="text-center">
            <a class="btn" :class="css" v-if="navigation" @click="navigate"><span v-if="icon" class="glyphicon glyphicon-share-alt rotate-270"></span> {{ navigation.title}}</a>
        </div>
    </div>
</template>
<script lang="ts">
    import { entityPartial } from "components/mixins"
    import * as _ from "lodash"

    export default {
        mixins: [entityPartial],
        name: "wb-actionButtons",
        computed: {
            navigation() {
                return this.$store.state.details.section.navigationState
            },
            icon() {
                if (this.navigation.isParentButton) {
                    return true
                } else {
                    return null
                }
            },
            css() {
                return [{
                    'btn-success': this.navigation.status == 1,
                    'btn-danger': this.navigation.status == -1,
                    'btn-default': this.navigation.isParentButton,
                    'btn-primary': !this.navigation.isParentButton
                }]
            },
            to() {
                return {
                    name: 'section',
                    params: {
                        sectionId: this.navigation.navigateToSection,
                        interviewId: this.$route.params.interviewId
                    }
                }
            }
        },
        methods: {
            navigate() {
                if (this.navigation.isParentButton) {
                    this.$store.dispatch("fetch/sectionRequireScroll", "#loc_" + this.$store.state.details.section.info.id)
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
