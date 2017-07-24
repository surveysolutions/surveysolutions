<template>
    <wb-question :question="$me" questionCssClassName=" audio-question">
        <div class="question-unit">
            <div class="options-group">
                <button type="button" class="btn btn-default btn-lg btn-action-questionnaire" v-if="!isRecording && !$me.isAnswered" v-on:click="startRecording">Tap to record audio</button>
                <div class="field answered" v-if="$me.isAnswered">
                    <ul class="block-with-data list-unstyled">
                        <li>10 minutes of audio recording</li>
                        <li>Recorded at: 09:42 AM</li>
                    </ul>
                    <wb-remove-answer @answerRemoved="answerRemoved" />
                </div>
                <div v-if="$me.isAnswered" class="action-btn-holder time-question">
                    <button v-if="!isRecording" v-on:click="startRecording" type="button" class="btn btn-default btn-lg btn-action-questionnaire">Record new</button>
                </div>
            </div>
        </div>
        <div class="modal fade" tabindex="-1" role="dialog">
            <div class="modal-dialog" role="document">
                <div class="modal-content">
                    <div class="modal-header">
                        <button type="button" v-on:click="cancelRecording" class="close" data-dismiss="modal" aria-label="Close">
                            <span aria-hidden="true"></span>
                        </button>
                        <h4 class="modal-title">Modal title</h4>
                        <h5 v-dateTimeFormatting v-html="$me.title"></h5>
                    </div>
                    <div class="modal-body">
                        <canvas class="analyser" width="100" height="100"></canvas>
                    </div>
                    <div class="modal-footer">
                        <div class="recordign-time">{{formattedTimer}}</div>
                        <button type="button" v-on:click="stopRecording" class="btn btn-primary" v-if="isRecording">Done</button>
                        <button type="button" v-on:click="cancelRecording" class="btn btn-link " data-dismiss="modal">Cancel</button>
                    </div>
                </div>
                <!-- /.modal-content -->
            </div>
            <!-- /.modal-dialog -->
        </div>
        <div class="modal-backdrop in" style="display: none"></div>
        <!-- /.modal -->
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
            isRecording: false,
            startDate: null,
            startRecordingTime: null,
            endRecordingTime: null,
            stopwatchInterval: null,
            formattedTimer: "00:00:00"
        }
    },
    methods: {
        answerRemoved() {
        },
        showModal() {
            var modal = $(this.$el).find(".modal");
            $(this.$el).find(".modal-backdrop").show()
            modal.addClass('in');
            modal.show();
        },
        closeModal() {
            var modal = $(this.$el).find(".modal");
            $(this.$el).find(".modal-backdrop").hide()
            modal.removeClass('in');
            modal.hide()
        },
        stopRecording() {
            this.terminateRecording();
            AudioRecorder.stop()
        },
        cancelRecording() {
            this.terminateRecording();
            AudioRecorder.cancel()
        },
        terminateRecording() {
            this.endRecordingTime = null
            this.isRecording = false
            this.closeModal()
            clearInterval(this.stopwatchInterval)
            this.formattedTimer = "00:00:00"
        },
        startRecording() {
            this.endRecordingTime = null
            this.showModal()
            const self = this
            AudioRecorder.initAudio({
                analyserEl: $(this.$el).find(".analyser")[0],
                startRecordingCallback: () => {
                    this.isRecording = true;
                    self.startRecordingTime = self.currentTime()
                    clearInterval(self.stopwatchInterval)
                    self.stopwatchInterval = setInterval(self.updateTimer, 50)
                },
                errorCallback: (e) => {
                    self.markAnswerAsNotSavedWithMessage("Audio initialization failed")
                    console.log(e)
                },
                doneCallback: (blob) => {
                    self.$store.dispatch('answerAudioQuestion', {
                        id: self.id,
                        file: blob,
                        length: self.endRecordingTime - self.startRecordingTime
                    })
                }
            })
        },
        currentTime() {
            return new Date().getTime();
        },
        updateTimer() {
            var diff = (this.currentTime() - this.startRecordingTime);

            var mins = Math.floor(diff / (1000 * 60));
            diff -= mins * (1000 * 60);

            var seconds = Math.floor(diff / (1000));
            diff -= seconds * (1000);

            var mseconds = Math.floor(diff / (10));

            this.formattedTimer = `${this.pad0(mins, 2)}:${this.pad0(seconds, 2)}:${this.pad0(mseconds, 2)}`;
        },
        pad0(value, count) {
            var result = value.toString();
            for (; result.length < count; --count)
                result = '0' + result;
            return result;
        }
    }
}
</script>
