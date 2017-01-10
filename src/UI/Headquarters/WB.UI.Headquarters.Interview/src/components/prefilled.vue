<template>
    <div>
        <form>
            <div class="form-group">
                <label for="interviewId">Interview id</label>
                <input type="text" id="interviewId" class="form-control" :value="interviewId">
            </div>
            <button type="button" @click="getPrefilledQuestions(interviewId)" class="btn btn-default">Get questions</button>
        </form>

        <br/><br/><br/>
        <hr/>
        <br/><br/><br/>

        <component v-for="question in prefilledQuestions" v-bind:is="question.entityType" v-bind:entity="question"></component>
    </div>
</template>

<script lang="ts">
    import { mapGetters, mapActions, mapState } from "vuex"

    export default {
        name: 'prefilled-view',
        computed: mapState({
            prefilledQuestions: state => state.prefilledQuestions,
            interviewId(state){
                return this.$router.params.id;
            }
        }),
        methods: mapActions(["getPrefilledQuestions"])
    }
</script>
