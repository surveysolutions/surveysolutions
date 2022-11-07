<template>
    <wb-question :question="$me"
        questionCssClassName="area-question">
        <div class="question-unit">
            <div class="options-group">
                <div class="field"
                    :class="{answered: $me.isAnswered}"
                    v-if="$me.isAnswered">
                    <div class="block-with-data">
                        <table class="list-unstyled">
                            <colgroup>
                                <col span="1"
                                    style="width: 30%;">
                                <col span="1"
                                    style="width: 70%;">
                            </colgroup>
                            <tbody>
                                <tr v-if="isPolygon">
                                    <td>{{this.$t('Pages.AreaQestion_Area')}}:</td>
                                    <td>{{$me.answer.area.toLocaleString()}} {{this.$t('Pages.AreaQestion_AreaUnitMeter')}}</td>
                                </tr>
                                <tr v-if="isPolygon || isPolyline">
                                    <td>{{this.$t('Pages.AreaQestion_Length')}}:</td>
                                    <td>{{$me.answer.length.toLocaleString()}} {{this.$t('Pages.AreaQestion_AreaMeter')}}</td>
                                </tr>
                                <tr>
                                    <td>{{this.$t('Pages.AreaQestion_Points')}}:</td>
                                    <td>{{$me.answer.selectedPoints.length.toLocaleString()}}</td>
                                </tr>
                                <tr v-if="!isManualMode">
                                    <td>{{this.$t('Pages.AreaQestion_RequestedAccuracy')}}:</td>
                                    <td>{{$me.answer.requestedAccuracy.toLocaleString()}}</td>
                                </tr>
                                <tr v-if="!coordinatesShown">
                                    <td>
                                        <button
                                            class="btn btn-link"
                                            type="button"
                                            @click="showCoordinates">{{this.$t('Details.Area_ShowCoordinates')}}</button>
                                    </td>
                                    <td></td>
                                </tr>
                                <tr v-else
                                    v-for="(selectedPoint, i) in $me.answer.selectedPoints"
                                    :key="i">
                                    <td colspan="2">
                                        <a
                                            v-bind:href="`${$config.googleMapsBaseUrl}/maps?q=${selectedPoint.latitude},${selectedPoint.longitude}`"
                                            :title="$t('WebInterviewUI.ShowOnMap')"
                                            target="_blank">{{selectedPoint.latitude}}, {{selectedPoint.longitude}}</a></td>
                                </tr>
                            </tbody>
                        </table>
                    </div>

                    <iframe width="100%"
                        height="250px"
                        title="geography"
                        frameborder="0"
                        :src="answerUrl"></iframe>
                </div>
                <div class="action-btn-holder">
                    <button
                        type="button"
                        disabled
                        class="btn btn-default btn-lg btn-action-questionnaire">{{ $t('WebInterviewUI.AreaRecord') }}</button>
                </div>
                <wb-lock />
            </div>
        </div>
    </wb-question>
</template>
<script lang="js">
import { entityDetails } from '../mixins'

export default {
    name: 'wb-geography',
    mixins: [entityDetails],
    data: function() {
        return {
            coordinatesShown: false,
        }
    },
    computed: {
        answerUrl() {
            return `${this.$store.getters.basePath}Interview/InterviewAreaFrame/${this.interviewId}?questionId=${this.$me.id}`
        },
        isPolygon() {
            return this.$me.type == 'Polygon'
        },
        isPolyline() {
            return this.$me.type == 'Polyline'
        },
        isMultiPoints() {
            return this.$me.type == 'Multipoint'
        },
        isManualMode() {
            return this.$me.mode == null || this.$me.mode == 'Manual'
        },
    },
    methods: {
        showCoordinates(){
            this.coordinatesShown = true
        },
    },
}
</script>
