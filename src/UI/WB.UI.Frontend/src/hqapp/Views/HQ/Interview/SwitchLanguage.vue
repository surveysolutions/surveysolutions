<template>
    <Select v-model="currentLanguage" :options="languages" />
</template>
<script>

import Select from '../../../components/Select.vue';

export default {
    components: { Select },
    data: function () {
        return { currentLanguage: null }
    },
    computed: {
        languages() {
            const langs = this.$store.state.webinterview.languages.map(element => {
                return { id: element, value: element }
            });
            return [{ id: null, value: this.$store.state.webinterview.originalLanguageName }].concat(langs)
        }
    },
    methods: {
        changeLanguage(language) {
            this.$store.dispatch('changeLanguage', { language })
        },
    },
    updated() {
    },
    mounted() {
        this.currentLanguage = this.$store.state.webinterview.currentLanguage || null
        this.$watch('currentLanguage', this.changeLanguage)
    },
}
</script>
