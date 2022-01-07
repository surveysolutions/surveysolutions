<template>
    <wb-question :question="$me"
        questionCssClassName="gps-question"
        :no-comments="noComments">
        <div class="question-unit">
            <div class="options-group">
                <div class="field"
                    :class="{answered: $me.isAnswered}"
                    v-if="$me.isAnswered">
                    <div class="block-with-data">
                        <a v-bind:href="goolgeMapUrl"
                            :title="$t('WebInterviewUI.ShowOnMap')"
                            target="_blank">
                            {{$me.answer.latitude}}, {{$me.answer.longitude}}
                        </a>
                    </div>
                    <button type="submit"
                        v-if="$me.acceptAnswer"
                        class="btn btn-link btn-clear"
                        @click="removeAnswer">
                        <span></span>
                    </button>
                </div>
                <div class="action-btn-holder gps-question">
                    <button type="button"
                        :disabled="!$me.acceptAnswer"
                        class="btn btn-default btn-lg btn-action-questionnaire"
                        @click="answerGpsQuestion">
                        {{ $t('WebInterviewUI.GPSRecord') }}
                    </button>

                    <button type="button"
                        v-if="$store.getters.pickLocationAllowed"
                        :disabled="!$me.acceptAnswer"
                        class="btn btn-default btn-lg btn-action-questionnaire pick-location marl"
                        @click="pickLocation">
                        {{ $t('WebInterviewUI.PickLocation') }}
                    </button>

                </div>
                <wb-lock />
            </div>
        </div>
    </wb-question>
</template>

<script lang="js">

import { entityDetails } from '../mixins'
import Vue from 'vue'
import moment from 'moment'
import Swal from 'sweetalert2'

class GpsAnswer {
    constructor(latitude,
        longitude,
        accuracy,
        altitude,
        timestamp) {
        this.latitude = latitude
        this.longitude = longitude
        this.accuracy = accuracy
        this.altitude = altitude
        this.timestamp = timestamp
    }
}
export default {
    name: 'Gps',
    mixins: [entityDetails],
    props: ['noComments'],
    data() {
        return {
            isInProgress: false,
            pickedLocation: null,
        }
    },
    computed: {
        goolgeMapUrl(){
            return `${this.$config.googleMapsBaseUrl}/maps?q=${this.$me.answer.latitude},${this.$me.answer.longitude}`
        },
    },
    methods: {
        removeAnswer() {
            this.$store.dispatch('removeAnswer', this.id)
        },
        answerGpsQuestion() {
            this.sendAnswer(() => {
                if (!('geolocation' in navigator)) {
                    this.markAnswerAsNotSavedWithMessage(this.$t('WebInterviewUI.GPSNotAvailable'))
                    return
                }

                if (this.isInProgress) return

                this.isInProgress = true
                this.$store.dispatch('fetchProgress', 1)

                navigator.geolocation.getCurrentPosition(
                    (position) => { this.onPositionDetermined(position, this.id) },
                    (error) => { this.onPositionDeterminationFailed(error) },
                    {
                        enableHighAccuracy: true,
                        timeout: 30000,
                        maximumAge: 60000,
                    })
            })
        },
        onPositionDetermined(position, questionId) {
            this.$store.dispatch('answerGpsQuestion', {
                identity: questionId,
                answer: new GpsAnswer(position.coords.latitude,
                    position.coords.longitude,
                    position.coords.accuracy,
                    position.coords.altitude,
                    new moment().valueOf()),
            }).then(() => {
                this.isInProgress = false
                this.$store.dispatch('fetchProgress', -1)
            })
        },
        onPositionDeterminationFailed(error) {
            var message = ''
            // Check for known errors
            switch (error.code) {
                case error.PERMISSION_DENIED:
                    message = this.$t('WebInterviewUI.GPSPermissionDenied')//"This website does not have permission to use the Geolocation API"
                    break
                case error.POSITION_UNAVAILABLE:
                    message = this.$t('WebInterviewUI.GPSPositionUnavailable') //"The current position could not be determined.";
                    break
                case error.TIMEOUT:
                    message = this.$t('WebInterviewUI.GPSTimeout') //"The current position could not be determined within the specified timeout period."
                    break
            }
            // If it is an unknown error, build a message that includes
            // information that helps identify the situation so that
            // the error handler can be updated.
            if (message == '') {
                var strErrorCode = error.code.toString()
                message = this.$t('WebInterviewUI.GPSError', { strErrorCode })  //"The position could not be determined due to an unknown error (Code: " + strErrorCode + ")."
            }

            this.markAnswerAsNotSavedWithMessage(message)
            this.$store.dispatch('fetchProgress', -1)
            this.isInProgress = false
        },
        pickLocation() {
            var self = this

            Swal.fire({
                title: self.$t('WebInterviewUI.PickLocation'),
                html: '<div id="locationPicker"><div style="height: 400px;" id="map_canvas"></div></div>',
                width: 600,
                confirmButtonText: self.$t('Common.Ok'),
                showCancelButton: true,
                showCloseButton: true,
                buttonsStyling: false,
                customClass:{
                    confirmButton: 'btn btn-primary',
                    cancelButton:'btn btn-link',
                    footer: 'modal-footer',
                    popup: 'modal-content',
                    header: 'modal-header',
                },

                cancelButtonText: self.$t('Common.Cancel'),

                didOpen: () => {
                    self.pickedLocation = null
                    var latlng = new google.maps.LatLng(-34.397, 150.644)

                    var mapOptions =
                    {
                        zoom: 14,
                        center:latlng,
                        streetViewControl: false,
                    }
                    const map = new google.maps.Map(
                        document.getElementById('map_canvas'), mapOptions)

                    if (navigator.geolocation) {
                        navigator.geolocation.getCurrentPosition((position) => {
                            var pos = {
                                lat: position.coords.latitude,
                                lng: position.coords.longitude,
                            }
                            map.setCenter(pos)
                        })
                    }
                    var pushpin = null
                    google.maps.event.addListener(map, 'click', function(event) {
                        placeMarker(event.latLng)
                        self.pickedLocation = {
                            latitude: pushpin.position.lat(),
                            longitude: pushpin.position.lng(),
                        }
                        if (event.placeId) {
                            event.stop() // prevent showing information about place
                        }
                    })

                    function placeMarker(location) {
                        if (pushpin == null)
                        {
                            pushpin = new google.maps.Marker({
                                position: location,
                                map: map,
                            })
                        }
                        else {
                            pushpin.setPosition(location)
                        }
                    }
                },

            }).then((result) => {
                if (result.isConfirmed) {

                    if(self.pickedLocation) {
                        self.onPositionDetermined({
                            coords: {
                                latitude: self.pickedLocation.latitude,
                                longitude: self.pickedLocation.longitude,
                            },
                        }, this.id)
                    }
                }
            })
        },
    },
}

</script>
