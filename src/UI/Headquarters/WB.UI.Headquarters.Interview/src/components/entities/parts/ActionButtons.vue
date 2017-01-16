<template>
    <div class="question">
        <div class="text-center">
            <router-link v-if="navigation" :to="to" class="btn" :class="css">
                <span v-if="icon" class="glyphicon glyphicon-share-alt rotate-270"></span> {{ navigation.title}}</router-link>
        </div>
    </div>
</template>
<script lang="ts">
    import { entityPartial } from "components/mixins"
    import * as _ from "lodash"
    import { prefilledSectionId } from "src/config"

    export default {
        mixins: [entityPartial],
        name: "wb-actionButtons",
        computed: {
            navigation() {
                return this.$store.state.details.section.navigationState
            },
            icon() {
                if (this.$route.query.questionId) {
                    return true
                } else {
                    return null
                }
            },
            css() {
                return [{
                    'btn-success': this.navigation.status == 1,
                    'btn-danger': this.navigation.status == -1,
                    'btn-default': this.$route.query.questionId,
                    'btn-primary': !this.$route.query.questionId
                }]
            },
            to() {
                let hash = ""

                if (this.$route.query.questionId) {
                    hash = "#" + this.$route.query.questionId
                }

                return {
                    name: 'section',
                    params: {
                        sectionId: this.navigation.navigateToSection,
                        interviewId: this.$route.params.interviewId
                    },
                    hash
                }
            }
        }
    }
</script>

<style>
.rotate-270{
    transform: rotate(270deg);
}
</style>
