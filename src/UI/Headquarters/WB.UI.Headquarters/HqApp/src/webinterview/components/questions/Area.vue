<template>
    <wb-question :question="$me" questionCssClassName="area-question">
        <div class="question-unit">
            <div class="options-group">
                <div class="field" :class="{answered: $me.isAnswered}" v-if="$me.isAnswered">
                    <div class="block-with-data">
                        <ul class="list-unstyled">
                            <li v-if="isPolygon">
                                {{this.$t('Pages.AreaQestion_Area')}}: {{$me.answer.area}} {{this.$t('Pages.AreaQestion_AreaUnitMeter')}}
                            </li>
                            <li v-if="isPolygon || isPolyline">
                                {{this.$t('Pages.AreaQestion_Length')}}: {{$me.answer.length}} {{this.$t('Pages.AreaQestion_AreaMeter')}}
                            </li>
                            <li v-if="isMultiPoints">
                                {{this.$t('Pages.AreaQestion_Points')}}: {{$me.answer.selectedPoints.length}}
                            </li>
                            <li v-if="!coordinatesShown">
                                <button class="btn btn-link" type="button" @click="showCoordinates">
                                    {{this.$t('Details.Area_ShowCoordinates')}}
                                </button>
                            </li>
                            <li v-else v-for="selectedPoint in $me.answer.selectedPoints">
                                <a v-bind:href="`${$config.googleMapsBaseUrl}/maps?q=${selectedPoint.latitude},${selectedPoint.longitude}`" :title="$t('WebInterviewUI.ShowOnMap')" target="_blank">{{selectedPoint.latitude}}, {{selectedPoint.longitude}}</a>
                            </li>
                        </ul>
                    </div>

                    <iframe width="100%" height="250px" frameBorder="0" :src="answerUrl"></iframe>
                </div>
                <div class="action-btn-holder">
                    <button type="button" disabled class="btn btn-default btn-lg btn-action-questionnaire" >{{ $t('WebInterviewUI.AreaRecord') }}</button>
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
            isPolygon() {
                return this.$me.type == 0
            },
            isPolyline() {
                return this.$me.type == 1
            },
            isMultiPoints() {
                return this.$me.type == 3
            }
        },
        methods: {
            showCoordinates(){
                this.coordinatesShown = true
            }
        }
    }
</script>
