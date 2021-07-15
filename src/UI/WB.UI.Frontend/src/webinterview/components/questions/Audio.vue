<template>
    <wb-question :question="$me"
        questionCssClassName=" audio-question">
        <div class="question-unit">
            <div class="options-group">
                <button type="button"
                    class="btn btn-default btn-lg btn-action-questionnaire"
                    v-if="!isRecording && !$me.isAnswered"
                    v-on:click="startRecording"
                    :disabled="!$me.acceptAnswer"><span></span>{{ $t('WebInterviewUI.AudioClickRecord')}}</button>
                <div class="field answered"
                    v-if="$me.isAnswered">
                    <ul class="block-with-data list-unstyled">
                        <li :id="answerHolderId">{{ $t("WebInterviewUI.AudioRecordingDuration", { humanizedLength: humanizedLength, formattedLength }) }}</li>
                    </ul>
                    <wb-remove-answer />
                </div>
                <div v-if="$me.isAnswered"
                    class="action-btn-holder time-question">
                    <audio controls
                        preload="auto"
                        style="width:300px"
                        :src="audioRecordPath">
                    </audio>
                </div>
                <wb-lock />
            </div>
        </div>
        <div class="modal fade"
            tabindex="-1"
            role="dialog">
            <div class="modal-dialog"
                role="document">
                <div class="modal-content"
                    :id="modalId">
                    <div class="modal-header">
                        <button type="button"
                            v-on:click="cancelRecording"
                            class="close"
                            data-dismiss="modal"
                            aria-label="Close">
                            <span aria-hidden="true"></span>
                        </button>
                        <h4 class="modal-title">
                            {{ $t("WebInterviewUI.AudioRecording") }}
                        </h4>
                        <h5 v-dateTimeFormatting
                            v-html="$me.title"></h5>
                    </div>
                    <div class="modal-body">
                        <canvas class="analyser"
                            width="100"
                            height="100"></canvas>
                    </div>
                    <div class="modal-footer">
                        <div class="recordign-time">
                            {{formattedTimer}}
                        </div>
                        <button type="button"
                            v-on:click="stopRecording"
                            class="btn btn-primary"
                            v-if="isRecording">
                            {{ $t("WebInterviewUI.Done") }}
                        </button>
                        <button type="button"
                            v-on:click="cancelRecording"
                            class="btn btn-link "
                            data-dismiss="modal">
                            {{ $t("WebInterviewUI.Cancel") }}
                        </button>
                    </div>
                </div>
                <!-- /.modal-content -->
            </div>
            <!-- /.modal-dialog -->
        </div>
        <div class="modal-backdrop in"
            style="display: none"></div>
        <!-- /.modal -->
        <li slot="sideMenu">
            <a :href="audioRecordPath"
                v-if="isRecorded">{{$t("Common.Download")}}</a>
        </li>
    </wb-question>
</template>
<script lang="js">

import { entityDetails } from '../mixins'
import moment from 'moment'
import '~/shared/misc/audioRecorder.js'
import api from '~/shared/api'

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
            formattedTimer: '00:00:00',
        }
    },
    computed: {
        answerHolderId(){
            return `audio_answer_${this.$me.id}`
        },
        modalId(){
            return `audio_dialog_${this.$me.id}`
        },
        audioRecordPath() {
            return api.resources.audioRecordUri(this.interviewId, this.$me.filename) + '#' + this.$me.updatedAt.getTime()
        },
        formattedLength() {
            if (this.$me.isAnswered){
                var d = moment.utc(this.$me.answer)
                return d.format('mm:ss')
            }
            return ''
        },
        humanizedLength() {
            if (this.$me.isAnswered){
                return moment.duration(this.$me.answer, 'milliseconds').humanize()
            }
            return ''
        },
        isRecorded() {
            return this.isRecording == false && this.$me.isAnswered
        },
    },
    methods: {
        showModal() {
            var modal = $(this.$el).find('.modal')
            $(this.$el).find('.modal-backdrop').show()
            modal.addClass('in')
            modal.show()
        },
        closeModal() {
            var modal = $(this.$el).find('.modal')
            $(this.$el).find('.modal-backdrop').hide()
            modal.removeClass('in')
            modal.hide()
        },
        stopRecording() {
            this.terminateRecording()
            AudioRecorder.stop()
        },
        cancelRecording() {
            this.terminateRecording()
            AudioRecorder.cancel()
        },
        terminateRecording() {
            this.isRecording = false
            this.closeModal()
            clearInterval(this.stopwatchInterval)
            clearInterval(this.maxDurationInterval)
            this.formattedTimer = '00:00:00'
        },
        startRecording() {
            this.sendAnswer(() => {
                this.showModal()
                const self = this
                AudioRecorder.initAudio({
                    analyserEl: $(this.$el).find('.analyser')[0],
                    startRecordingCallback: () => {
                        self.isRecording = true
                        self.startRecordingTime = self.currentTime()
                        clearInterval(self.stopwatchInterval)
                        clearInterval(self.maxDurationInterval)
                        self.stopwatchInterval = setInterval(self.updateTimer, 31)
                        self.maxDurationInterval = setInterval(self.stopRecording, self.maxDuration)
                    },
                    errorCallback: (e) => {
                        self.markAnswerAsNotSavedWithMessage(this.$t('WebInterviewUI.AudioInitializationFailed'))
                        this.closeModal()
                    },
                    doneCallback: (blob, duration) => {
                        self.$store.dispatch('answerAudioQuestion', {
                            identity: self.id,
                            file: blob,
                            duration : duration,
                        })
                    },
                })
            })
        },
        currentTime() {
            return new Date().getTime()
        },
        updateTimer() {
            var diff = moment.utc(this.currentTime() - this.startRecordingTime)
            this.formattedTimer = diff.format('mm:ss:SS')
        },
    },
}
</script>
