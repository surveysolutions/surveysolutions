<template>
    <div class="attachment">
        <div v-if="localContentType === 'image' && thumbPath" class="image-zoom-box image-wrapper" :class="customCssClass">
            <img :src="thumbPath" alt="custom photo" class="zoomImg" @load="imageLoaded" @click="showModal(true)"
                :style="previewStyle" />
            <portal to="body">
                <div class="modal-img" v-if="modal" :style="modalView" @click="showModal(false)">
                    <span class="close-zoomming-img">×</span>
                    <img class="modal-img-content" :src="fullPath" alt />
                    <span class="caption"></span>
                </div>
            </portal>
        </div>
        <div v-if="localContentType === 'audio'">
            <div class="instructions-wrapper">
                <a class="btn btn-link" :href="contentUrl" target="_blank">
                    {{ $t("Common.Download") }}
                </a>
            </div>
            <div>
                <audio controls preload="auto" :src="contentUrl">{{ $t('WebInterviewUI.MultimediaNotSupported')
                    }}</audio>
            </div>
        </div>
        <div v-if="localContentType === 'video'">
            <div class="instructions-wrapper">
                <a class="btn btn-link" :href="contentUrl" target="_blank">
                    {{ $t("Common.Download") }}
                </a>
            </div>
            <div>
                <video controls preload="auto" style="width:300px" :src="contentUrl">{{
            $t('WebInterviewUI.MultimediaNotSupported') }}</video>
            </div>
        </div>
        <div v-if="localContentType === 'pdf'">
            <div class="instructions-wrapper">
                <a class="btn btn-link" :href="contentUrl" target="_blank">
                    {{ $t("Common.Download") }}
                </a>
            </div>
        </div>
    </div>
</template>
<script lang="js">
import axios from 'axios'
import { startsWith } from 'lodash'

function appendSearchParam(uri, name, value) {
    const url = new URL(uri, window.location.origin);
    url.searchParams.append(name, value);
    return url.toString();
}

export default {
    emits: ['imageLoaded'],
    data() {
        return {
            modal: false,
            contentType: '',
            onEscape: null,
        }
    },
    props: {
        filename: {
            type: String,
        },
        contentId: {
            type: String,
            required: false,
        },
        attachmentName: {
            type: String,
            required: false,
        },
        interviewId: {
            type: String,
            required: false,
        },
        cache: {
            type: Number,
        },
        thumb: {
            type: String,
            required: false,
        },
        image: {
            type: String,
        },
        customCssClass: {
            type: String,
            required: false,
        },
        previewOnly: {
            type: Boolean,
            required: false,
            default: false,
        },
    },
    mounted() {
        const self = this
        this.onEscape = (e) => {
            if (self.modal && e.keyCode === 27) {
                self.showModal(false)
            }
        }
        document.addEventListener('keydown', this.onEscape)

        return this.fetchContentType()
    },
    beforeUnmount() {
        document.removeEventListener('keydown', this.onEscape)
    },
    watch: {
        contentId() {
            this.fetchContentType()
        },
        attachmentName() {
            this.fetchContentType()
        },
    },
    computed: {
        contentUrl() {
            if (this.contentId) return `${this.$config.imageGetBase}/Content?interviewId=${this.interviewId}&contentId=${this.contentId}`
            if (this.attachmentName) return `${this.$config.imageGetBase}/Attachment?interviewId=${this.interviewId}&attachment=${this.attachmentName}`
            return null
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
            if (this.thumb) return this.thumb
            if (this.filename) return `${this.$config.imageGetBase}/Image/${this.filename}`
            if (this.contentId) return `${this.$config.imageGetBase}/Content?interviewId=${this.interviewId}&contentId=${this.contentId}`
            if (this.attachmentName) return `${this.$config.imageGetBase}/Attachment?interviewId=${this.interviewId}&attachment=${this.attachmentName}`
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
                    cursor: 'auto',
                }
            }

            return {}
        },
        localContentType() {
            if (startsWith(this.contentType, 'image'))
                return 'image'
            if (startsWith(this.contentType, 'application/pdf'))
                return 'pdf'
            if (startsWith(this.contentType, 'audio'))
                return 'audio'
            if (startsWith(this.contentType, 'video'))
                return 'video'
            return ''
        },
        modalView() {
            return {
                display: this.modal ? 'block' : 'none',
            }
        },
    },
    methods: {
        async fetchContentType() {
            if (this.thumb || this.filename) {
                this.contentType = 'image'
            }
            else {
                const response = await axios.head(this.contentUrl)
                this.contentType = response.headers['content-type']
            }
        },
        appendCache(uri) {
            if (this.cache)
                return appendSearchParam(uri, 'cache', this.cache)
            return uri;
        },
        showModal(show) {
            if (this.previewOnly)
                return
            this.modal = show
        },
        imageLoaded() {
            this.$emit('imageLoaded')
        },
    },
    name: 'wb-attachment',
}

</script>
