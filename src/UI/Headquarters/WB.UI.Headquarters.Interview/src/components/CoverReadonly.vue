<template>
    <div class="unit-section complete-section first-last-chapter">
        <div class="unit-title">
            <wb-humburger></wb-humburger>
            <h3>Cover</h3>
        </div>
        <div class="wrapper-info">
            <div class="container-info">
                <h2>{{title}}</h2>
                <p>
                    <b>Please provide answers to all questions to the extent possible. 
                            Any answers you provide are sent to our system right away.
                            If you experience a communication disruption you can return to the questionnaire 
                            by following the same link you prowided so far retained for you.
                            You can then continue by submitting new answers or revising earlier answers.</b>
                </p>
            </div>
        </div>
        <div v-for="question in questions"
             class="wrapper-info">
            <div class="container-info">
                <p v-html="question.title"></p>
                <p>
                    <b>{{question.answer}}</b>
                </p>
            </div>
        </div>
        <NavigationButton id="NavigationButton" :target="firstSectionId"></NavigationButton>
    </div>
</template>

<script lang="ts">
    export default {
        name: "cover-readonly-view",
        beforeMount() {
            this.fetch()
        },
        computed: {
           title() {
               return this.$store.state.questionnaireTitle
           },
           questions() {
               return this.$store.state.samplePrefilledInfo.questions
           },
           firstSectionId() {
               return this.$store.state.firstSectionId
           }
        },
        methods: {
            fetch() {
                this.$store.dispatch("fetchSamplePrefilled")
            }
        }
    }
</script>
