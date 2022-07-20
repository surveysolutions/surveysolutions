<template>
    <div class="unit-title break-line"
        v-if="showBreadcrumbs">
        <ol class="breadcrumb">
            <li v-for="breadcrumb in entities"
                :key="breadcrumb.target">
                <a href="javascript:void(0)"
                    @click="navigate(breadcrumb)">
                    <span v-html="breadcrumb.title"></span><span v-if="breadcrumb.isRoster && !breadcrumb.hasCustomRosterTitle"> - <i>{{getRosterTitle(breadcrumb.rosterTitle)}}</i></span>
                </a>
            </li>
        </ol>
        <h3 v-html="title"></h3>
    </div>
</template>
<script lang="js">
export default {
    name: 'breadcrumbs-view',

    props: {
        showHumburger: {
            type: Boolean,
            default: false,
        },
    },

    mounted() {
        this.fetchBreadcrumbs()
    },

    watch: {
        '$store.state.route.params.sectionId'() {
            this.fetchBreadcrumbs()
        },
    },
    computed: {
        info() {
            return this.$store.state.webinterview.breadcrumbs
        },
        showBreadcrumbs() {
            return this.$store.state.route.params.sectionId != null && this.info.title != null
        },
        entities() {
            if (!this.info) return {}
            return this.info.breadcrumbs
        },
        title(){
            var title = this.info.title

            if(this.info.isRoster && !this.info.hasCustomRosterTitle)
                title += '<span> - <i>' + this.getRosterTitle(this.info.rosterTitle) + '</i></span>'

            return title
        },
    },
    methods: {
        navigate(breadcrumb) {
            if (breadcrumb.scrollTo) {
                this.$store.dispatch('sectionRequireScroll', { id: breadcrumb.scrollTo })
            }

            this.$router.push({
                name: 'section',
                params: {
                    sectionId: breadcrumb.target,
                },
            })
        },
        fetchBreadcrumbs() {
            this.$store.dispatch('fetchBreadcrumbs')
        },
        getRosterTitle(title) {
            return title ? title : '[...]'
        },
    },
}

</script>
