<template>
    <div class="unit-section">
        <search-breabcrumbs :sections="search.sections"/>

        <a href="javascript:void(0)"
            v-for="link in search.questions"
            :key="link.target"
            class="question short-row"
            @click="navigate(link)">
            <span
                v-html="link.title"/>
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
            var self = this
            this.$router.push(
                { name: 'section', params: { sectionId: this.search.sectionId }, hash: '#' + link.target },
                undefined,
                () => { self.$store.dispatch('sectionRequireScroll', {id: '#' + link.target}) }
            )
        },
    },
}
</script>
