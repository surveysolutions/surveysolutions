<template>
    <wb-question :question="$me" questionCssClassName=" audio-question">
        <div class="question-unit">
            <div class="options-group">
                <button type="button" class="btn btn-default btn-lg btn-action-questionnaire"
                    v-if="!isRecording && !$me.isAnswered" v-on:click="startRecording">{{ $t('AudioClickRecord')}}</button>
                <div class="field answered" v-if="$me.isAnswered">
                    <ul class="block-with-data list-unstyled">
                        <li>{{ $t("AudioRecordingDuration", { humanizedLength: humanizedLength, formattedLength }) }}</li>
                    </ul>
                    <wb-remove-answer @answerRemoved="answerRemoved" />
                </div>
                <div v-if="$me.isAnswered" class="action-btn-holder time-question">
                    <button v-if="!isRecording" v-on:click="startRecording" type="button"
                        class="btn btn-default btn-lg btn-action-questionnaire">{{ $t("AudioRecordNew") }}</button>
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
                        <h4 class="modal-title">{{ $t("AudioRecording") }}</h4>
                        <h5 v-dateTimeFormatting v-html="$me.title"></h5>
                    </div>
                    <div class="modal-body">
                        <canvas class="analyser" width="100" height="100"></canvas>
                    </div>
                    <div class="modal-footer">
                        <div class="recordign-time">{{formattedTimer}}</div>
                        <button type="button" v-on:click="stopRecording" class="btn btn-primary" v-if="isRecording">{{ $t("Done") }}</button>
                        <button type="button" v-on:click="cancelRecording" class="btn btn-link " data-dismiss="modal">{{ $t("Cancel") }}</button>
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
<script lang="js">
import { entityDetails } from "components/mixins"
import * as $ from 'jquery'
import * as moment from "moment"
const AudioRecorder = new window.AudioRecorder

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
            maxDuration: 3 * 60 * 1000,
            maxDurationInterval: null,
            formattedTimer: "00:00:00"
        }
    },
    computed: {
            formattedLength() {
                if (this.$me.isAnswered){
                    var d = moment.utc(this.$me.answer);
                    return d.format("mm:ss");
                }
                return ''
            },
            humanizedLength() {
                if (this.$me.isAnswered){
                    return moment.duration(this.$me.answer, 'milliseconds').humanize();
                }
                return ''
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
            this.isRecording = false
            this.closeModal()
            clearInterval(this.stopwatchInterval)
            clearInterval(this.maxDurationInterval)
            this.formattedTimer = "00:00:00"
        },
        startRecording() {
            this.showModal()
            const self = this
            AudioRecorder.initAudio({
                analyserEl: $(this.$el).find(".analyser")[0],
                startRecordingCallback: () => {
                    this.isRecording = true;
                    self.startRecordingTime = self.currentTime()
                    clearInterval(self.stopwatchInterval)
                    clearInterval(self.maxDurationInterval)
                    self.stopwatchInterval = setInterval(self.updateTimer, 31)
                    self.maxDurationInterval = setInterval(self.stopRecording, self.maxDuration)
                },
                errorCallback: (e) => {
                    self.markAnswerAsNotSavedWithMessage(this.$t("AudioInitializationFailed"))
                    console.log(e)
                },
                doneCallback: (blob) => {
                    self.$store.dispatch('answerAudioQuestion', {
                        id: self.id,
                        file: blob
                    })
                }
            })
        },
        currentTime() {
            return new Date().getTime();
        },
        updateTimer() {
            var diff = moment.utc(this.currentTime() - this.startRecordingTime);
            this.formattedTimer = diff.format("mm:ss:SS");
        }
    }
}
</script>
