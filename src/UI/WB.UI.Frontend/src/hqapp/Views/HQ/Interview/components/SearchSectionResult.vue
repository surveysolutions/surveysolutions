<template>
    <div class="unit-section">
        <search-breabcrumbs :sections="search.sections"/>

        <router-link
            v-for="link in search.questions"
            :key="link.target"
            active-class=""
            :to="{ name: 'section', params: { sectionId: search.sectionId }, hash: '#' + link.target}"
            class="question short-row">
            <span
                v-html="link.title"/>
        </router-link>
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
}
</script>
