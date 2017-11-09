<template>
    <div id="questionsList" class="unit-section" :class="sectionClass">
        <SectionLoadingProgress />
        <Breadcrumbs />
        <component v-for="entity in entities" :key="entity.identity" :is="entity.entityType" :id="entity.identity"></component>
    </div>
</template>

<script lang="js">
    import SectionProgress from "./SectionLoadProgress"
    import Vue from 'vue'
    
    async function checkSectionPermission(to) {
          if (to.name === "section") {
                return await Vue.$api.call(api => api.isEnabled(to.params["sectionId"]))
          }
    }

    export default {
        name: 'section-view',

        beforeMount() {
            this.loadSection()
        },

        async beforeRouteEnter (to, from, next) {
            if(checkSectionPermission(to)) {
                next(vm => vm.$store.dispatch("changeSection", to.params.sectionId))
                return;
            }
            
            next(false);
        },

        async beforeRouteUpdate (to, from, next) {
            if(checkSectionPermission(to)) {
                this.$store.dispatch("changeSection", to.params.sectionId)
                next();
                return;
            }
            
            next(false);
        },

        watch: {
            ["$route.params.sectionId"]() {
                 this.loadSection()
            }
        },

        computed: {
            entities() {
                return this.$store.state.webinterview.entities
            },
            fetchProgress() {
                return this.$store.state.webinterview.fetch.inProgress
            },
            info() {
                return this.$store.state.webinterview.breadcrumbs
            },
            sectionClass() {
                if (this.info) {
                    return [
                        {
                            'complete-section': this.info.status == "Completed",
                            'section-with-error': this.info.status == "Invalid"
                        }
                    ]
                }
                return []
            }
        },
        methods: {
            loadSection() {
                this.$store.dispatch("fetchSectionEntities")
            }
        },
        components: {
            SectionLoadingProgress: SectionProgress
        }
    }
</script>
