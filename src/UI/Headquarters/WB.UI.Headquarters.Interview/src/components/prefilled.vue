<template>
    <div>
        <h1>Prefilled questions</h1>
        <button @click="getPrefilledQuestions($route.params.id)">Get questions</button>
        <ul>
            <li v-for="question in prefilledQuestions">
                {{question.title}}
            </li>
        </ul>
    </div>
</template>

<script lang="ts">
    import { mapGetters, mapActions } from "vuex"

    export default {
        name: 'prefilled-view',
        computed: mapGetters(["prefilledQuestions"]),
        methods: mapActions(["getPrefilledQuestions"])
    }
</script>
