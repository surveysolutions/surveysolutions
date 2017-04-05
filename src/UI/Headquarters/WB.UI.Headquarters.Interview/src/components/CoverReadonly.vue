<template>
    <div class="unit-section first-last-chapter">
        <div class="unit-title">
            <wb-humburger></wb-humburger>
            <h3>{{title}}</h3>
        </div>
    </div>
</template>

<script lang="ts">
    export default {
        name: "cover-readonly-view",
        computed: {
           title() {
               return this.$store.state.questionnaireTitle
           }
        }
    }
</script>
