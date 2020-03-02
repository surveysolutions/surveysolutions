<template>
    <select ref="select"
        v-model="currentLanguage">
        <option :value="null"
            v-html="$store.state.webinterview.originalLanguageName" />
        <option :key="language.OriginalLanguageName"
            v-for="language in $store.state.webinterview.languages"
            v-html="language" />
    </select>
</template>
<script>
export default {
    data: function() {
        return { currentLanguage: null }
    },
    methods: {
        changeLanguage(language) {
            this.$store.dispatch('changeLanguage', { language })
        },
    },
    updated() {
        $(this.$refs.select).selectpicker('refresh')
    },
    mounted() {
        this.currentLanguage = this.$store.state.webinterview.currentLanguage || null
        this.$watch('currentLanguage', this.changeLanguage)
    },
}
</script>
