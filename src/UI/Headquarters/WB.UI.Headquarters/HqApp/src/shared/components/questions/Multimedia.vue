<template>
    <wb-question :question="$me" questionCssClassName=" multimedia-question">
        <div class="question-unit">
            <div class="options-group">
                <div class="field" :class="{ answered: $me.isAnswered}" v-if="answerVisible">
                    <wb-attachment :filename="$me.answer" :thumb="uploadingImage"></wb-attachment>
                    <wb-remove-answer @answerRemoved="answerRemoved" />
                </div>
                <input name="file" ref="uploader" v-show="false" accept="image/*" type="file"
                    @change="onFileChange" class="btn btn-default btn-lg btn-action-questionnaire" />
                <button type="button" class="btn btn-default btn-lg btn-action-questionnaire"
                    v-if="!$me.isAnswered && !$me.fetchState" @click="$refs.uploader.click()">{{ $t("WebInterviewUI.PhotoUpload") }}</button>
            </div>
        </div>
    </wb-question>
</template>
<script lang="js">
    import { entityDetails } from "../mixins"
    import * as $ from 'jquery'

    const imageFileSizeLimit = 30 * 1024 * 1024; // mb

    export default {
        name: 'picture-question',
        mixins: [entityDetails],
        data() {
            return {
                uploadingImage: null
            }
        },
        computed: {
            answerVisible() {
                if(this.$me.answer){
                    return true
                }

                if(this.$me.validity.isValid) return this.uploadingImage != null

                return false;
            }
        },
        watch:{
            "$me.answer"() {
                this.uploadingImage = null
            }
        },
        methods: {
            answerRemoved() {
                this.$refs.uploader.type = ''
                this.$refs.uploader.type = 'file'
            },
            onFileChange(e) {
                var files = e.target.files || e.dataTransfer.files;

                if (!files.length) {
                    return;
                }

                this.createImage(files[0]);
            },
            createImage(file) {
                if (file.size > imageFileSizeLimit) {
                    // Image is too big to upload. Please, choose an image less than 30 Mb
                    this.markAnswerAsNotSavedWithMessage(this.$t("WebInterviewUI.PhotoTooBig"))
                    return
                }

                const image = new Image()
                const self = this
                image.onload = () => {
                    if (image.width) {
                        self.cleanValidity()

                        self.$store.dispatch('answerMultimediaQuestion', {
                            id: self.id,
                            file: self.$refs.uploader.files[0]
                        })

                        const reader = new FileReader()
                        reader.onload = (e) => {
                            const imageUri = (e.target ).result
                            self.uploadingImage = imageUri
                        }

                        reader.readAsDataURL(file)
                    } else {
                        // Only image files are allowed to upload
                        self.markAnswerAsNotSavedWithMessage(this.$t("WebInterviewUI.PhotoIsNotImage") )
                    }
                };

                image.src = URL.createObjectURL(file);
            }
        }
    }

</script>
