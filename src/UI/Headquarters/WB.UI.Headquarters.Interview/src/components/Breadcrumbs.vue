<template>
    <div class="unit-title" v-if="showBreadcrumbs">
        <ol class="breadcrumb">
            <li v-for="breadcrumb in entities" :key="breadcrumb.target">
                 <a href="javascript:void(0)" @click="navigate(breadcrumb)">{{ breadcrumb.title}}</a>
            </li>
        </ol>
        <h3>{{info.title}}</h3>
    </div>
</template>
<script lang="ts">
    export default {
        name: 'breadcrumbs-view',
        beforeMount() {
            this.fetchBreadcrumbs()
        },
        watch: {
            $route(from, to) {
                this.fetchBreadcrumbs()
            }
        },
        computed: {
            info() {
                return this.$store.state.breadcrumbs
            },
            showBreadcrumbs() {
                return this.$route.params.sectionId != null && this.info.title != null
            },
            entities() {
                if (!this.info) return {}
                return this.info.breadcrumbs
            }
        },
        methods: {
            navigate(breadcrumb) {
                if (breadcrumb.scrollTo) {
                    this.$store.dispatch("sectionRequireScroll", { id: breadcrumb.scrollTo })
                }

                this.$router.push({
                    name: "section",
                    params: {
                        sectionId: breadcrumb.target
                    }
                })
            },
            fetchBreadcrumbs() {
                this.$store.dispatch("fetchBreadcrumbs")
            }
        }
    }
</script>
