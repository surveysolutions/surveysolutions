<template>
    <wb-question :question="$me"
        questionCssClassName="area-question">
        <div class="question-unit">
            <div class="options-group">
                <div class="field"
                    :class="{answered: $me.isAnswered}"
                    v-if="$me.isAnswered">
                    <div class="block-with-data">
                        <table class="list-unstyled"
                            role="presentation">
                            <colgroup>
                                <col span="1"
                                    style="width: 30px;">
                                <col span="1"
                                    style="width: 40px;">
                                <col span="1">
                            </colgroup>
                            <tbody>
                                <tr v-if="isPolygon">
                                    <td colspan="2">
                                        {{this.$t('Pages.AreaQestion_Area')}}:
                                    </td>
                                    <td>{{$me.answer.area.toLocaleString()}} {{this.$t('Pages.AreaQestion_AreaUnitMeter')}}</td>
                                </tr>
                                <tr v-if="isPolygon || isPolyline">
                                    <td colspan="2">
                                        {{this.$t('Pages.AreaQestion_Length')}}:
                                    </td>
                                    <td>{{$me.answer.length.toLocaleString()}} {{this.$t('Pages.AreaQestion_AreaMeter')}}</td>
                                </tr>
                                <tr>
                                    <td colspan="2">
                                        {{this.$t('Pages.AreaQestion_Points')}}:
                                    </td>
                                    <td>{{$me.answer.selectedPoints.length.toLocaleString()}}</td>
                                </tr>
                                <tr v-if="!isManualMode && $me.answer.requestedAccuracy">
                                    <td colspan="2">
                                        {{this.$t('Pages.AreaQuestion_RequestedAccuracy')}}:
                                    </td>
                                    <td>{{$me.answer.requestedAccuracy.toLocaleString()}}</td>
                                </tr>
                                <tr v-if="isAutoMode && $me.answer.requestedFrequency">
                                    <td colspan="2">
                                        {{this.$t('Pages.AreaQuestion_RequestedFrequency')}}:
                                    </td>
                                    <td>{{$me.answer.requestedFrequency.toLocaleString()}}</td>
                                </tr>
                                <tr v-if="!coordinatesShown">
                                    <td colspan="3">
                                        <button
                                            class="btn btn-link"
                                            type="button"
                                            @click="showCoordinates">{{this.$t('Details.Area_ShowCoordinates')}}</button>
                                    </td>
                                </tr>
                                <template v-else>
                                    <tr>
                                        <td colspan="3">
                                            &nbsp;
                                        </td>
                                    </tr>
                                    <tr>
                                        <td colspan="3">
                                            {{this.$t('Details.Area_Coordinates')}}:
                                        </td>
                                    </tr>
                                    <tr>
                                        <td></td>
                                        <td colspan="2">
                                            <span>Latitude &nbsp; Longitude</span>
                                        </td>
                                    </tr>
                                    <tr
                                        v-for="(selectedPoint, i) in $me.answer.selectedPoints"
                                        :key="i">
                                        <td>{{ i + 1 }}</td>
                                        <td colspan="2">
                                            <a
                                                v-bind:href="`${$config.googleMapsBaseUrl}/maps?q=${selectedPoint.latitude},${selectedPoint.longitude}`"
                                                :title="$t('WebInterviewUI.ShowOnMap')"
                                                target="_blank">{{selectedPoint.latitude.toFixed(6)}}, {{selectedPoint.longitude.toFixed(6)}}</a></td>
                                    </tr>
                                </template>
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
        isAutoMode() {
            return this.$me.mode != null && this.$me.mode == 'Automatic'
        },
    },
    methods: {
        showCoordinates(){
            this.coordinatesShown = true
        },
    },
}
</script>
