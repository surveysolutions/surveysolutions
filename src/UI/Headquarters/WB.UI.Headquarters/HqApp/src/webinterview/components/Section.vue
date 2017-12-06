<template>
    <div id="questionsList" class="unit-section" :class="sectionClass">
        <SectionLoadingProgress />
        <Breadcrumbs :showHumburger="showHumburger" />
        <component v-for="entity in entities" :key="entity.identity" :is="entity.entityType" :id="entity.identity"></component>
    </div>
</template>

<script lang="js">
    import SectionProgress from "./SectionLoadProgress"
    import Vue from 'vue'
    import { GroupStatus } from "./questions"
    
    async function checkSectionPermission(to) {
          if (to.name === "section") {
                return await Vue.$api.call(api => api.isEnabled(to.params["sectionId"]))
          }
    }

    export default {
        name: 'section-view',

        props:{
            showHumburger: {
                type: Boolean,
                default: true
            }
        },

        beforeMount() {
            this.loadSection()
        },

        mounted() {
            if(this.$route.hash){
                this.$store.dispatch("sectionRequireScroll", { id: this.$route.hash })
            }
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
            hasError() {
                return this.info.validity && this.info.validity.isValid === false;
            },
            sectionClass() {
                if (this.info) {
                    return [
                        {
                            'complete-section': this.info.status == GroupStatus.Completed && !this.hasError,
                            'section-with-error': this.hasError
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
