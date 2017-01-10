<template>
    <div v-if="$store.state.questionnaire">
        <h1>
            You are about to start interview for questionnaire: '{{$store.state.questionnaire.title}}'
        </h1>
        <button type="submit" class="btn btn-success" @click="startInterview">Start</button>
    </div>
</template>
<script lang="ts">
    export default {
        name: 'start-view',
        beforeMount() {
            this.loadQuestionnaire()
        },
        methods: {
            loadQuestionnaire() {
                this.$store.dispatch('loadQuestionnaire', this.$route.params.id)
            },
            startInterview() {
                this.$store.dispatch('startInterview', this.$route.params.id)
            },
            openInterview() {
                if (this.interviewId) {
                    this.$router.push({
                        name: "prefilled",
                        params: { id: this.interviewId }
                    });
                }
            }
        }
    }
</script>
