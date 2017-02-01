<template>
    <wb-question :question="$me" questionCssClassName="gps-question">
        <div class="question-unit">
            <div class="options-group">
                <div class="field" :class="{answered: $me.isAnswered}" v-if="$me.isAnswered">
                    <div class="map" id="map">
                        <div>
                            <img v-bind:src="googleMapPosition" draggable="false" />
                        </div>
                    </div>
                    <div class="block-with-data">{{$me.answer.longitude}}, {{$me.answer.latitude}}</div>
                    <button type="submit" class="btn btn-link btn-clear" @click="removeAnswer"><span></span></button>
                </div>
                <div class="action-btn-holder gps-question">
                    <button type="button" class="btn btn-default btn-lg btn-action-questionnaire" @click="answerGpsQuestion">Tap to record GPS</button>
                </div>
            </div>
        </div>
    </wb-question>
</template>
<script lang="ts">
    import { entityDetails } from "components/mixins"

    class GpsAnswer implements IGpsAnswer {
        latitude: number
        longitude: number
        accuracy: number
        altitude: number
        timestamp: number
        constructor(latitude: number,
            longitude: number,
            accuracy: number,
            altitude: number,
            timestamp: number) {
            this.latitude = latitude
            this.longitude = longitude
            this.accuracy = accuracy
            this.altitude = altitude
            this.timestamp = timestamp
        }
    }
    export default {
        name: "Gps",
        mixins: [entityDetails],
        computed: {
            googleMapPosition() {
                return 'https://maps.googleapis.com/maps/api/staticmap?center=' + this.$me.answer.latitude + ',' + this.$me.answer.longitude
                    + '&zoom=14&scale=0&size=385x200&markers=color:blue|label:O|' + this.$me.answer.latitude + ',' + this.$me.answer.longitude
                    + '&key=AIzaSyCSzVuKZ30UhOYVrseHIRsK4XNiO3Tpd3Q'
            }
        },
        methods: {
            markAnswerAsNotSavedWithMessage(message) {
                const id = this.id
                this.$store.dispatch("setAnswerAsNotSaved", { id, message })
            },
            removeAnswer() {
                this.$store.dispatch("removeAnswer", this.id)
            },
            answerGpsQuestion() {
                if (!('geolocation' in navigator))
                {
                    this.markAnswerAsNotSavedWithMessage('Your browser does not support receiving Geolocation')
                    return
                }

                this.$store.dispatch("fetchProgress", 1)
                
                var gl = navigator.geolocation
                var viewModel = this
                gl.getCurrentPosition(
                    function (position) {
                        viewModel.$store.dispatch('answerGpsQuestion', {
                            identity: viewModel.$me.id,
                            answer: new GpsAnswer(position.coords.latitude, position.coords.longitude, position.coords.accuracy, position.coords.altitude, position.timestamp)
                        })
                    },
                    function (error) {
                        viewModel.markAnswerAsNotSavedWithMessage(error.message)
                        viewModel.$store.dispatch("fetchProgress", -1)
                    },
                    {
                        enableHighAccuracy: true,
                        timeout: 5000,
                        maximumAge: 0
                    })
            },
            a: async function(){

            }

        }
    }
</script>