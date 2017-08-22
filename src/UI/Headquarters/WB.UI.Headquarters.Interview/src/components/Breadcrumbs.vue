<template>
    <div class="unit-title break-line" v-if="showBreadcrumbs">
        <wb-humburger></wb-humburger>
        <ol class="breadcrumb">
            <li v-for="breadcrumb in entities" :key="breadcrumb.target">
                <a href="javascript:void(0)" @click="navigate(breadcrumb)">{{ breadcrumb.title}} <span v-if="breadcrumb.isRoster"> - <i>{{getRosterTitle(breadcrumb.rosterTitle)}}</i></span> </a>
            </li>
        </ol>
        <h3>{{info.title}} <span v-if="info.isRoster"> - <i>{{getRosterTitle(info.rosterTitle)}}</i></span></h3>
    </div>
</template>
<script lang="js">
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
            },
            getRosterTitle(title) {
                return title ? title : "[...]"
            }
        }
    }

</script>
