<template>
    <wb-question :question="$me" questionCssClassName=" audio-question">
        <div class="question-unit">
            <div class="options-group">
                <button type="button" class="btn btn-default btn-lg btn-action-questionnaire" v-if="!isRecording" v-on:click="requestAccessAndStartRecording">Tap to record audio</button>

                <button type="button" class="btn btn-link" v-if="isRecording" v-on:click="stopRecording">Done</button>
            </div>
        </div>
        <canvas id="analyser" width="100" height="100"></canvas>
        <canvas id="wavedisplay" width="100" height="50"></canvas>
        <audio ref="audiofile" preload="auto"></audio>
    </wb-question>
</template>
<script lang="ts">
import { entityDetails } from "components/mixins"
import * as $ from 'jquery'
const AudioRecorder = new ((window as any).AudioRecorder)

export default {
    name: 'Audio',
    mixins: [entityDetails],
    data() {
        return {
            isRecording: false
        }
    },
    computed: {
    },
    methods: {
        stopRecording() {
            this.isRecording = false;
            AudioRecorder.stop();
        },
        requestAccessAndStartRecording() {
            const self = this
            AudioRecorder.initAudio({
                analyserEl: $(this.$el).find("#analyser")[0],
                wavedisplayEl: $(this.$el).find("#wavedisplay")[0],
                errorCallback: (e) => {
                    self.markAnswerAsNotSavedWithMessage("Audio initialization failed")
                    console.log(e)
                },
                doneCallback: (blob) => {
                    var url = URL.createObjectURL(blob);
                    var audioElement = $(self.$el).find("audio")[0];

                    audioElement.controls = true;
                    audioElement.src = url;

                    self.$store.dispatch('answerAudioQuestion', {
                        id: self.id,
                        file: blob
                    })
                }
            })

            //AudioRecorder.start()
            this.isRecording = true;
        },
        mounted() {

        },

    }
}
</script>
