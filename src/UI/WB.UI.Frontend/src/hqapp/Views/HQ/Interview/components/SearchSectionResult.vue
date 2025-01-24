<template>
    <div class="unit-section">
        <search-breabcrumbs :sections="search.sections" />

        <a href="javascript:void(0)" v-for="link in search.questions" :key="link.target" class="question" :class='{
            "short-row": search.sectionId != "critical-rules"
        }' @click="navigate(link)">
            <span v-dompurify-html="link.title" />
        </a>
    </div>
</template>

<script>
import SearchBreabcrumbs from './SearchBreabcrumbs'

export default {
    props: {
        search: {
            type: Object,
            required: true,
        },
    },
    components: { SearchBreabcrumbs },
    methods: {
        navigate(link) {
            if (this.search.sectionId == 'critical-rules')
                return;

            const coverPageId = this.$config.coverPageId == undefined
                ? this.$config.model.coverPageId
                : this.$config.coverPageId
            const routeName = coverPageId == this.search.sectionId
                ? 'cover'
                : 'section'

            this.$router.push(
                { name: routeName, params: { sectionId: this.search.sectionId }, hash: '#' + link.target },
                undefined,
                () => { this.$store.dispatch('sectionRequireScroll', { id: '#' + link.target }) }
            )
        },
    },
}
</script>
