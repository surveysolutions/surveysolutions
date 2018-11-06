<template>

    <div>
        <div v-if="contentType.startsWith('image')" class="image-zoom-box image-wrapper" :class="customCssClass">
            <img :src="thumbPath" alt="custom photo" class="zoomImg" @click="showModal(true)" :style="previewStyle">
            <div class="modal-img" :style="modalView" @click="showModal(false)">
                <span class="close-zoomming-img">×</span>
                <img class="modal-img-content" :src="fullPath" alt="">
                <span class="caption"></span>
            </div>
        </div>
        <div v-if="contentType.startsWith('audio')">
            <audio controls preload="auto" style="width:300px" :src="contentUrl">
            </audio>
        </div>
        <div v-if="contentType.startsWith('video')">
            <video controls preload="auto" style="width:300px" :src="contentUrl">
            </video>
        </div>
        <div v-if="contentType.startsWith('application/pdf')">
            <div class="instructions-wrapper">
                <a class="btn btn-link" :href="contentUrl" target="_blank">{{$t("Common.Download")}}</a>
            </div>

        </div>
    </div>
</template>
<script lang="js">
    import axios from "axios"
    import appendquery from "append-query"

    function appendSearchParam(uri, name, value) {
        const args = {
            [name]: value
        } // keep in separate line to make IE happy
        return appendquery(uri, args);
    }

    export default {
        data() {
            return {
                modal: false,
                contentType: ""
            }
        },
        props: {
            filename: {
                type: String
            },
            contentId: {
                type: String,
                required: true
            },
            interviewId: {
                type: String,
                required: true
            },
            cache: {
                type: Number
            },
            thumb: {
                type: String,
                required: false
            },
            image: {
                type: String
            },
            customCssClass: {
                type: String,
                required: false
            },
            previewOnly: {
                type: Boolean,
                required: false,
                default: false
            }
        },
        mounted() {
            return this.fetchContentType()
        },
        computed: {
            contentUrl() {
                return `${this.$config.imageGetBase}/Content?interviewId=${this.interviewId}&contentId=${this.contentId}`
            },
            thumbPath() {
                if (this.isPreview) return this.imageThumb
                return this.appendCache(this.imageThumb)
            },
            fullPath() {
                if (this.isPreview) return this.imageFull
                return this.appendCache(this.imageFull)
            },
            imageThumb() {
                if (this.thumb) return this.thumb;
                if (this.filename) return `${this.$config.imageGetBase}/Image/${this.filename}`
                if (this.contentId) return `${this.$config.imageGetBase}/Content?interviewId=${this.interviewId}&contentId=${this.contentId}`
                return null
            },
            isPreview() {
                return this.imageThumb != null && this.imageThumb.lastIndexOf('data:image/') == 0
            },
            imageFull() {
                if (this.image) return this.image
                if (!this.isPreview && this.imageThumb) return `${this.imageThumb}&fullSize`
                return null
            },
            previewStyle() {
                if (this.isPreview) {
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
            async fetchContentType() {
                const response = await axios.head(this.contentUrl)
                this.contentType = response.headers["content-type"]
            },
            appendCache(uri) {
                return appendSearchParam(uri, 'cache', this.cache)
            },
            showModal(show) {
                if (this.previewOnly)
                    return;
                this.modal = show
            }
        },
        name: "wb-attachment"
    }

</script>
