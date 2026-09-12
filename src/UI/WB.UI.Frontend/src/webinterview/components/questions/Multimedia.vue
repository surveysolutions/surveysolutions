<template>
    <wb-question :question="$me"
        questionCssClassName=" multimedia-question">
        <div class="question-unit">
            <div class="options-group">
                <div class="field"
                    :class="{ answered: $me.isAnswered}"
                    v-if="answerVisible">
                    <wb-attachment :filename="$me.answer"
                        :thumb="uploadingImage"
                        :cache="cache"></wb-attachment>
                    <wb-remove-answer @answerRemoved="answerRemoved" />
                </div>
                <input name="file"
                    ref="uploader"
                    v-show="false"
                    accept=".heic,.heif,.svg,image/*"
                    type="file"
                    @change="onFileChange"
                    class="btn btn-default btn-lg btn-action-questionnaire" />
                <button type="button"
                    class="btn btn-default btn-lg btn-action-questionnaire"
                    :disabled="!$me.acceptAnswer"
                    v-if="!$me.isAnswered && !inFetchState"
                    @click="$refs.uploader.click()">{{ $t("WebInterviewUI.PhotoUpload") }}</button>
                <wb-lock />
            </div>
        </div>
    </wb-question>
</template>
<script lang="js">
import { entityDetails } from '../mixins'

const imageFileSizeLimit = 30 * 1024 * 1024 // mb
const additionalImageExtensions = ['.heic', '.heif', '.svg']

function hasSupportedImageExtension(fileName) {
    if (!fileName)
        return false

    const lowerCaseFileName = fileName.toLowerCase()
    return additionalImageExtensions.some(extension => lowerCaseFileName.endsWith(extension))
}

function isImageFile(file) {
    return file?.type?.startsWith('image/') || hasSupportedImageExtension(file?.name)
}

export default {
    name: 'picture-question',
    mixins: [entityDetails],
    data() {
        return {
            uploadingImage: null,
        }
    },
    computed: {
        cache() {
            return this.$me.answerTimeUtc == null ? null : new Date(this.$me.answerTimeUtc).getTime()
        },
        errorMessage() {
            return this.$me.validity.errorMessage
        },
        answerVisible() {
            if(this.$me.answer){
                return true
            }

            if(this.$me.validity.isValid) return this.uploadingImage != null

            return false
        },
    },

    watch:{
        '$me.answer'() {
            this.uploadingImage = null
        },
        errorMessage(val) {
            if (val) {
                this.uploadingImage = null
                const uploader = this.$refs.uploader
                if (uploader) {
                    uploader.type = ''
                    uploader.type = 'file'
                }
            }
        },
    },

    methods: {
        answerRemoved() {
            const uploader = this.$refs.uploader
            if (uploader) {
                uploader.type = ''
                uploader.type = 'file'
            }
        },
        onFileChange(e) {
            this.sendAnswer(() => {
                const files = e.target.files || e.dataTransfer.files

                if (!files.length) {
                    return
                }

                this.createImage(files[0])
            })
        },
        createImage(file) {
            if (file.size > imageFileSizeLimit) {
                // Image is too big to upload. Please, choose an image less than 30 Mb
                this.markAnswerAsNotSavedWithMessage(this.$t('WebInterviewUI.PhotoTooBig'))
                return
            }

            if (!isImageFile(file)) {
                this.markAnswerAsNotSavedWithMessage(this.$t('WebInterviewUI.PhotoIsNotImage'))
                return
            }

            this.cleanValidity()
            this.$store.dispatch('answerMultimediaQuestion', {
                identity: this.id,
                file,
            })

            const image = new Image()
            const self = this
            const objectUrl = URL.createObjectURL(file)

            image.onerror = () => {
                URL.revokeObjectURL(objectUrl)
            }

            image.onload = () => {
                if (('naturalHeight' in this && this.naturalHeight + this.naturalWidth !== 0)
                    || (this.width + this.height !== 0)) {
                    const reader = new FileReader()
                    reader.onload = (e) => {
                        const imageUri = (e.target ).result
                        self.uploadingImage = imageUri
                    }

                    reader.readAsDataURL(file)
                }

                URL.revokeObjectURL(objectUrl)
            }

            image.src = objectUrl
        },
    },
}

</script>
