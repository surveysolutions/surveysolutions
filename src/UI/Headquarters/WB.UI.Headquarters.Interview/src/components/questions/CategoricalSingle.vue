<template>
    <div class="question">
        <div class="question-editor text-question">
            <wb-title :entityId="entity.identity" />
            <wb-instructions :entityId="entity.Identity" />
        </div>
    </div>
</template>
<script lang="ts">
    export default {
        name: 'CategoricalSingle',
        props: ['entity'],
        mounted() {
            this.fetch()
        },
        methods: {
            fetch() {
                this.$store.dispatch('fetchSingleOptionQuestion', this.entity)
            }
        }
    }
</script>
