<template>
    <div class="image-zoom-box image-wrapper" :class="customCssClass">
        <img :src="imageThumb" alt="custom photo" class="zoomImg"
            @click="showModal(true)" :style="previewStyle">
        <div class="modal-img" :style="modalView" @click="showModal(false)">
            <span class="close-zoomming-img">×</span>
            <img class="modal-img-content" :src="imageFull" alt="">
            <span class="caption"></span>
        </div>
    </div>
</template>
<script lang="js">
    // import { imageGetBase } from "src/config"

    export default {
        data() {
            return {
                modal: false
            }
        },
        props: {
            filename: {type: String},
            contentId: {type: String},
            interviewId: {type: String},
            thumb: { type: String }, // optional
            image: { type: String },
            customCssClass:{}
        },
        computed: {
            imageThumb() {
                if(this.thumb) return this.thumb;
                if(this.filename) return `${this.$config.imageGetBase}/Image/${this.filename}`
                if(this.contentId) return `${this.$config.imageGetBase}/Content?interviewId=${this.interviewId}&contentId=${this.contentId}`
                return null
            },
            isPreview(){
                return this.imageThumb != null && this.imageThumb.lastIndexOf('data:image/') == 0
            },
            imageFull() {
                if(this.image) return this.image
                if(!this.isPreview && this.imageThumb) return `${this.imageThumb}&fullSize`
                return null
            },
            previewStyle() {
                if(this.isPreview){
                    return {
                        cursor: "auto"
                    };
                }

                return {}
            },
            modalView() {
                return {
                    display: this.modal ? 'block' : 'none'
                }
            }
        },
        methods: {
            showModal(show) {
                this.modal = show
            },
        },
        name: "wb-attachment"
    }

</script>
