<template>
    <wb-question :question="$me" questionCssClassName=" multimedia-question">
        <div class="question-unit">
            <div class="options-group">
                <div class="field answered" v-if="imageSrc || $me.uploadingImage">
                    <div class="image-zoom-box image-wrapper">
                        <img :src="imageSrc" alt="custom photo" class="zoomImg" @click="showModal(true)" v-if="!uploadingImageVisible">
                        <img :src="$me.uploadingImage" alt="custom photo" class="zoomImg" style="cursor: auto" v-if="uploadingImageVisible">
                        <div class="modal-img" :style="modalView" @click="showModal(false)">
                            <span class="close-zoomming-img">×</span>
                            <img class="modal-img-content" :src="imageSrcFullSize" alt="">
                            <span class="caption"></span>
                        </div>
                    </div>
                    <wb-remove-answer @answerRemoved="answerRemoved" />
                </div>
                <input name="file" ref="uploader" v-show="false" accept="image/*" type="file" @change="onFileChange" class="btn btn-default btn-lg btn-action-questionnaire"
                />
                <button type="button" class="btn btn-default btn-lg btn-action-questionnaire" v-if="!$me.isAnswered && !$me.fetchState" @click="$refs.uploader.click()">Tap to take a photo</button>
            </div>
        </div>
    </wb-question>
</template>
<script lang="ts">
    import { entityDetails } from "components/mixins"
    import * as $ from 'jquery'
    import { imageGetBase } from "src/config"

    export default {
        name: 'picture-question',
        mixins: [entityDetails],
        data() {
            return {
                modal: false
            }
        },
        computed: {
            uploadingImageVisible() {
                return this.$me.uploadingImage
            },
            modalView() {
                return {
                    display: this.modal ? 'block' : 'none'
                }
            },
            imageSrc() {
                if (this.$me.isAnswered) {
                    return imageGetBase + this.$me.answer
                } else {
                    return ''
                }
            },
            imageSrcFullSize() {
                if (this.$me.isAnswered && this.modal) {
                    return imageGetBase + this.$me.answer + "&fullSize=true"
                } else {
                    return ''
                }
            }
        },
        methods: {
            showModal(show) {
                this.modal = show
            },
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
                var image = new Image();
                image.onload = () => {
                    if (image.width) {
                        const reader = new FileReader()
                        reader.onload = (e) => {
                            this.$store.dispatch('answerMultimediaQuestion', {
                                id: this.id,
                                file: this.$refs.uploader.files[0]
                            })
                            this.$me.uploadingImage = (e.target as any).result
                        }

                        reader.readAsDataURL(file)
                    } else {
                        this.markAnswerAsNotSavedWithMessage("Only image files are allowed to upload")
                    }
                };

                image.src = URL.createObjectURL(file);
            }
        }
    }

</script>
