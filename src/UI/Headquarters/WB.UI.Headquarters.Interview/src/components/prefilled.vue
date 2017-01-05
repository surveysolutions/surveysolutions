<template>
    <div>
        <h1>Prefilled questions</h1>
        <input id="interviewId" type="text" v-model="interviewId" />

        <button @click="getPrefilledQuestions($route.params.id)">Get questions</button>
        <ul>
            <li v-for="question in prefilledQuestions">
                <component v-bind:is="question.EntityType"></component>
            </li>
        </ul>
    </div>
</template>

<script lang="ts">
    import { mapGetters, mapActions } from "vuex"

    export default {
        name: 'prefilled-view',
        computed: mapGetters(["prefilledQuestions"]),
        methods: mapActions(["getPrefilledQuestions"]),
          data: () => {
            return {
                interviewId: "66990b89-1891-4117-b4ff-65da7d933d00"
            }
        },
    }
</script>
