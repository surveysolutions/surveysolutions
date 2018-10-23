<template>
    <wb-question :question="$me" questionCssClassName="gps-question" :no-comments="noComments">
        <div class="question-unit">
            <div class="options-group">
                <div class="field" :class="{answered: $me.isAnswered}" v-if="$me.isAnswered">
                    <div class="map" id="map">
                        <div>
                            <img v-bind:src="googleMapPosition" draggable="false" />
                        </div>
                    </div>
                    <div class="block-with-data"><a v-bind:href="goolgeMapUrl" :title="$t('WebInterviewUI.ShowOnMap')" target="_blank">{{$me.answer.latitude}}, {{$me.answer.longitude}}</a></div>
                    <button type="submit" v-if="$me.acceptAnswer" class="btn btn-link btn-clear" @click="removeAnswer"><span></span></button>
                </div>
                <div class="action-btn-holder gps-question">
                    <button type="button" :disabled="!$me.acceptAnswer" class="btn btn-default btn-lg btn-action-questionnaire" @click="answerGpsQuestion">{{ $t('WebInterviewUI.GPSRecord') }}</button>
                </div>
                <wb-lock />
            </div>
        </div>
    </wb-question>
</template>

<script lang="js">

    import { entityDetails } from "../mixins"
    
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
        name: "Gps",
        mixins: [entityDetails],
        props: ['noComments'],
        data() {
            return {
                isInProgress: false
            }
        },
        computed: {
            googleMapPosition() {
                return `${this.$config.googleMapsApiBaseUrl}/maps/api/staticmap?center=${this.$me.answer.latitude},${this.$me.answer.longitude}`
                    + `&zoom=14&scale=0&size=385x200&markers=color:blue|label:O|${this.$me.answer.latitude},${this.$me.answer.longitude}`
                    + `&key=${this.$config.googleApiKey}`
            },
            goolgeMapUrl(){
                return `${this.$config.googleMapsBaseUrl}/maps?q=${this.$me.answer.latitude},${this.$me.answer.longitude}`
            }
        },
        methods: {
            removeAnswer() {
                this.$store.dispatch("removeAnswer", this.id)
            },
            answerGpsQuestion() {
                this.sendAnswer(() => {
                    if (!('geolocation' in navigator)) {
                        this.markAnswerAsNotSavedWithMessage(this.$t("WebInterviewUI.GPSNotAvailable"))
                        return
                    }

                    if (this.isInProgress) return

                    this.isInProgress = true
                    this.$store.dispatch("fetchProgress", 1)

                    navigator.geolocation.getCurrentPosition(
                        (position) => { this.onPositionDetermined(position, this.id) },
                        (error) => { this.onPositionDeterminationFailed(error) },
                        {
                            enableHighAccuracy: true,
                            timeout: 30000,
                            maximumAge: 60000
                        })
                });
            },
            onPositionDetermined(position, questionId) {
                this.$store.dispatch('answerGpsQuestion', {
                    identity: questionId,
                    answer: new GpsAnswer(position.coords.latitude, position.coords.longitude, position.coords.accuracy, position.coords.altitude, position.timestamp)
                })
                this.isInProgress = false
            },
            onPositionDeterminationFailed(error) {
                var message = "";
                // Check for known errors
                switch (error.code) {
                    case error.PERMISSION_DENIED:
                        message = this.$t("WebInterviewUI.GPSPermissionDenied")//"This website does not have permission to use the Geolocation API"
                        break;
                    case error.POSITION_UNAVAILABLE:
                        message = this.$t("WebInterviewUI.GPSPositionUnavailable") //"The current position could not be determined.";
                        break
                    case error.TIMEOUT:
                        message = this.$t("WebInterviewUI.GPSTimeout") //"The current position could not be determined within the specified timeout period."
                        break
                }
                // If it is an unknown error, build a message that includes
                // information that helps identify the situation so that
                // the error handler can be updated.
                if (message == "") {
                    var strErrorCode = error.code.toString();
                    message = this.$t("WebInterviewUI.GPSError", { strErrorCode })  //"The position could not be determined due to an unknown error (Code: " + strErrorCode + ")."
                }

                this.markAnswerAsNotSavedWithMessage(message)
                this.$store.dispatch("fetchProgress", -1)
                this.isInProgress = false
            }
        }
    }

</script>
