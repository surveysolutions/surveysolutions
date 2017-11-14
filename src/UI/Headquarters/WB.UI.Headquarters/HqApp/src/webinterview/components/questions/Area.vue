<template>
    <wb-question :question="$me" questionCssClassName="area-question">
        <div class="question-unit">
            <div class="options-group">
                <div class="field" :class="{answered: $me.isAnswered}">
                    <div class="block-with-data">
                        <ul class="list-unstyled">
                            <li>
                                {{this.$t('Pages.AreaQestion_Area')}}: {{area}} {{this.$t('Pages.AreaQestion_AreaUnitMeter')}}
                            </li>
                            <li v-if="!coordinatesShown">
                                <button class="btn btn-link" type="button" @click="showCoordinates">
                                    {{this.$t('Details.Area_ShowCoordinates')}}
                                </button>
                            </li>
                            <li v-else>{{this.$me.coordinates}}</li>
                        </ul>
                    </div>

                    <iframe width="100%" height="250px" frameBorder="0" :src="answerUrl"></iframe>
                </div>
                <wb-lock />
            </div>
        </div>
    </wb-question>
</template>
<script lang="js">
    import { entityDetails } from "../mixins"

    export default {
        name: 'Area',
        mixins: [entityDetails],
        data: function() {
            return {
                coordinatesShown: false
            }
        },
        computed: {
            answerUrl() {
                return `${this.$store.getters.basePath}Interview/InterviewAreaFrame/${this.interviewId}?questionId=${this.$me.id}`
            },
            area() {
                return this.$me.answer
            }
        },
        methods: {
            showCoordinates(){
                this.coordinatesShown = true
            }
        }
    }
</script>
