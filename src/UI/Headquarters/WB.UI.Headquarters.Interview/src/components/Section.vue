<template>
    <div v-if="section">
        <component v-for="entity in section.entities" v-bind:is="entity.entityType" v-bind:entity="entity"></component>
        <!--<h1>
            You are browsing section component
        </h1>-->



    </div>
</template>
<script lang="ts">

    export default {
        name: 'section-view',
        beforeMount() {
            this.loadSection()
        },
        computed: {
            section() {
                return this.$store.state.section
            }
        },
        methods: {
            loadSection() {
                this.$store.dispatch("loadSection", this.$route.params.sectionId)
            }
        }
    }
</script>
