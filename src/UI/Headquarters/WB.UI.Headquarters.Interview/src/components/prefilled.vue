<template>
    <div>
        <form>
            <div class="form-group">
                <label for="interviewId">Interview id</label>
                <input type="text" id="interviewId" class="form-control" v-model="interviewId">
            </div>
            <button type="button" @click="getPrefilledQuestions(interviewId)" class="btn btn-default">Get questions</button>
        </form>
        <component v-for="question in prefilledQuestions" v-bind:is="question.EntityType"></component>
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
