<template>
    <wb-question :question="$me" questionCssClassName=" audio-question">
        <div class="question-unit">
            <div class="options-group">
                <button type="button" class="btn btn-default btn-lg btn-action-questionnaire" v-if="!isRecording" v-on:click="startRecording">Tap to record audio</button>
                <div class="field answered" v-if="$me.isAnswered">
                    <ul class="block-with-data list-unstyled">
                        <li>10 minutes of audio recording</li>
                        <li>Recorded at: 09:42 AM</li>
                        <li>Noise level: 76db</li>
                    </ul>
                    <wb-remove-answer @answerRemoved="answerRemoved" />
                </div>
                <div class="action-btn-holder time-question">
                    <button type="button" class="btn btn-default btn-lg btn-action-questionnaire">Record new</button>
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
                        <div class="recordign-time">{{recordingTime}}</div>
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
            startRecordingTime: null
        }
    },
    computed: {
        recordingTime(){
            if (!isRecording)
                return "00:00:00";
            return "00:00:01";
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
            this.isRecording = false;
            this.closeModal();
            AudioRecorder.stop();
        },
        cancelRecording() {
            this.isRecording = false;
            AudioRecorder.cancel();
            this.closeModal();
        },
        startRecording() {
            this.showModal();
            const self = this
            AudioRecorder.initAudio({
                analyserEl: $(this.$el).find(".analyser")[0],
                startRecordingCallback: () => {
                    self.startRecordingTime = new Date().getTime();
                },
                errorCallback: (e) => {
                    self.markAnswerAsNotSavedWithMessage("Audio initialization failed")
                    console.log(e)
                },
                doneCallback: (blob) => {
                    self.$store.dispatch('answerAudioQuestion', {
                        id: self.id,
                        file: blob,
                        length: new Date().getTime() - self.startRecordingTime
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
