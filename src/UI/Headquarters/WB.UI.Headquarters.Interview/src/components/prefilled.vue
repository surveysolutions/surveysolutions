<template>
    <div>
        <form>
            <div class="form-group">
                <label for="interviewId">Interview id</label>
                <input type="text" id="interviewId" class="form-control" v-model="interviewId">
            </div>
            <button type="button" @click="getPrefilledQuestions(interviewId)" class="btn btn-default">Get questions</button>
        </form>

        <br/><br/><br/>
        <hr/>
        <br/><br/><br/>

        <component v-for="question in prefilledQuestions" v-bind:is="question.EntityType" v-bind:entity="question"></component>
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
                interviewId: "a82cb5a5-1519-40a6-9f40-733dbb749487"
            }
        },
    }
</script>
