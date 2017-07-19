<template>
    <wb-question :question="$me" questionCssClassName=" audio-question">
        <div class="question-unit">
            <div class="options-group">
                <button type="button" class="btn btn-default btn-lg btn-action-questionnaire" v-if="!isRecording" v-on:click="startRecording">Tap to record audio</button>
            </div>
        </div>
        <div class="modal fade" tabindex="-1" role="dialog">
            <div class="modal-dialog" role="document">
                <div class="modal-content">
                    <div class="modal-header">
                        <h4 class="modal-title">Modal title</h4>
                        <h5 v-dateTimeFormatting v-html="$me.title"></h5>
                    </div>
                    <div class="modal-body">
                        <canvas class="analyser" width="100" height="100"></canvas>
                    </div>
                    <div class="modal-footer">
                        <span></span>
                        <button type="button" v-on:click="cancelRecording" class="btn btn-link " data-dismiss="modal">Cancel</button>
                        <button type="button" v-on:click="stopRecording" class="btn btn-primary" v-if="isRecording" >Done</button>
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
            startDate: null
        }
    },
    computed: {
    },
    methods: {
        showModal(){
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
                errorCallback: (e) => {
                    self.markAnswerAsNotSavedWithMessage("Audio initialization failed")
                    console.log(e)
                },
                doneCallback: (blob) => {
                    self.$store.dispatch('answerAudioQuestion', {
                        id: self.id,
                        file: blob,
                        length: 10
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
