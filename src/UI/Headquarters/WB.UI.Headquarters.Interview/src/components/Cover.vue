<template>
    <div class="unit-section complete-section">
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
                            by following the same link you provided so far retained for you.
                            You can then continue by submitting new answers or revising earlier answers.
                    </b>
                </p>
            </div>
        </div>

        <template v-for="question in questions">
            <div class="wrapper-info" v-if="question.isReadonly">
                <div class="container-info" :id="question.identity">
                    <p v-html="question.title"></p>
                    <p>
                        <b v-if="question.type == 'Gps'">
                            <a :href="getGpsUrl(question)" target="_blank">{{question.answer}}</a>
                        </b>
                        <b v-else-if="question.type == 'DateTime'" v-dateTimeFormatting v-html="question.answer">
                        </b>
                        <b v-else>{{question.answer}}</b>
                    </p>
                </div>
            </div>
            <component v-else :key="question.identity" v-bind:is="question.type" v-bind:id="question.identity"></component>

        </template>


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
            },
            getGpsUrl(question: IReadonlyPrefilledQuestion) {
                return `http://maps.google.com/maps?q=${question.answer}`
            }
        }
    }
</script>
