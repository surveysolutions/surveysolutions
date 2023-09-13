import { marked } from 'marked'
import mdEditor from '@/hqapp/components/MdEditor'
import Logo from './_Logo.vue'
import Vue from 'vue'

export default {
    props: {
        logoUrl: String,
        hasLogo: Boolean,
        questionnaireId: String,
        questionnaireTitle: String,
        webInterviewPageMessages: Object,
    },

    components: { Logo, mdEditor },

    mounted() {
        var self = this
        this.$eventHub.$on('settings:page:active', ({ titleType, messageType }) => {
            if (titleType && self.$refs[titleType]) {
                self.$refs[titleType].resize()
            }

            if (messageType && self.$refs[messageType]) {
                self.$refs[messageType].refresh()
            }
        })
    },

    methods: {
        isDirty(formName) {
            const form = this.fields[formName]
            const keys = Object.keys((this.fields || {})[formName] || {})
            return keys.some(key => form[key].dirty || form[key].changed)
        },
        dummy() {
            return false
        },
        previewHtml(text) {
            var html = marked(this.previewText(text))
            return html
        },
        previewText(text) {
            if (text == null)
                return ''

            return text
                .replace(/%SURVEYNAME%/g, this.questionnaireTitle)
                .replace(/%QUESTIONNAIRE%/g, this.questionnaireTitle)
        },

        async savePageTextEditMode(scope, titleType, messageType, buttonText) {
            var self = this
            var validationResult = await this.$validator.validateAll(scope)
            if (validationResult) {
                var editTitleText = this.webInterviewPageMessages[titleType]
                var editDescriptionText = this.webInterviewPageMessages[messageType]
                self.$store.dispatch('showProgress')

                try {
                    await this.$hq.WebInterviewSettings.updatePageMessage(
                        self.questionnaireId, titleType, editTitleText.text, messageType, editDescriptionText.text,
                        buttonText, buttonText !== undefined ? this.webInterviewPageMessages[buttonText].text : undefined)

                    editTitleText.cancelText = editTitleText.text
                    editDescriptionText.cancelText = editDescriptionText.text

                    if (buttonText !== undefined)
                        self.webInterviewPageMessages[buttonText].cancelText = self.webInterviewPageMessages[buttonText].text
                    self.$validator.reset(scope)
                }
                catch (error) {
                    Vue.config.errorHandler(error, self)
                }
                finally {
                    self.$store.dispatch('hideProgress')
                }
            }
        },
        cancelPageTextEditMode(scope, titleType, messageType, buttonText) {
            var editTitleText = this.webInterviewPageMessages[titleType]
            var editDescriptionText = this.webInterviewPageMessages[messageType]
            editTitleText.text = editTitleText.cancelText
            editDescriptionText.text = editDescriptionText.cancelText

            if (buttonText !== undefined) {
                var editButtonText = this.webInterviewPageMessages[buttonText]
                editButtonText.text = editButtonText.cancelText
            }
            this.$validator.reset(scope)
        },
    },
}