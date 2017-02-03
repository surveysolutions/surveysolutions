<template>
    <wb-question :question="$me" questionCssClassName=" multimedia-question">
        <div class="question-unit">
            <div class="options-group">
                <div class="field answered" v-if="imageSrc">
                    <div class="image-zoom-box image-wrapper">
                        <img :src="imageSrc" alt="custom photo" class="zoomImg" @click="showModal(true)">
                        <div class="modal-img" :style="modalView" @click="showModal(false)">
                            <span class="close-zoomming-img">×</span>
                            <img class="modal-img-content" :src="imageSrcFullSize" alt="">
                            <span class="caption"></span>
                        </div>
                    </div>
                    <wb-remove-answer @answerRemoved="answerRemoved" />
                </div>
                <form method="post" enctype="multipart/form-data" class="action-btn-holder photo-question">
                    <input name="file" ref="uploader" type="file" @change="onFileChange" class="btn btn-default btn-lg btn-action-questionnaire">Tap
                    to take a photo</button>
                </form>
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
                imageData: null,
                modal: false
            }
        },
        computed: {
            modalView() {
                return {
                    display: this.modal ? 'block' : 'none'
                }
            },
            imageSrc() {
                if (this.$me.isAnswered) {
                    return imageGetBase + this.$me.answer
                } else {
                    return '' // this.imageData
                }
            },
            imageSrcFullSize() {
                if (this.$me.isAnswered && this.modal) {
                    return imageGetBase + this.$me.answer + "&fullSize=true"
                } else {
                    return '' // this.imageData
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
                this.$store.dispatch('answerMultimediaQuestion', {
                    id: this.id,
                    file: this.$refs.uploader.files[0]
                })
            }
        }
    }

</script>
