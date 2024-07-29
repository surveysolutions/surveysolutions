<template>
    <div :class="itemClass" class="overview-item">
        <div class="date">
            <div v-if="hasDate">
                {{ answerDate }}
            </div>
            <div v-if="hasDate">
                {{ answerTime }}
            </div>
        </div>
        <div ref="itemContent" class="item-content" @click="showAdditionalDetails">
            <h4>
                <span v-html="item.title"></span>
                <template v-if="item.rosterTitle != null"><span> - </span>
                    <i v-if="item.rosterTitle != null" v-html="item.rosterTitle"></i>
                </template>
            </h4>
            <div class="answer" v-if="hasAttachment">
                <wb-attachment :contentId="attachmentContentId" :interviewId="interviewId" :previewOnly="true"
                    customCssClass="static-text-image"></wb-attachment>
            </div>
            <div class="answer" v-if="item.state != 'Unanswered'">
                <div v-if="item.controlType === 'image'">
                    <wb-attachment :filename="item.answer" :previewOnly="true"></wb-attachment>
                </div>
                <div v-else-if="item.controlType === 'audio'">
                    <audio controls preload="auto" style="width:300px" :src="audioRecordPath">
                    </audio>
                </div>
                <div v-else-if="item.controlType === 'area'">
                    <iframe width="100%" height="250px" frameBorder="0" :src="areaAnswerUrl"></iframe>
                </div>
                <div v-else-if="item.controlType === 'map'">
                    <a v-bind:href="goolgeMapUrl" :title="$t('WebInterviewUI.ShowOnMap')" target="_blank">
                        {{ gpsAnswer.latitude }}, {{ gpsAnswer.longitude }}
                    </a>
                </div>
                <div v-else-if="item.controlType === 'variable'">
                    {{ item.value }}
                </div>
                <div v-else>
                    {{ item.answer }}
                </div>
            </div>
            <div class="btn-link" v-if="item.state == 'Unanswered' && item.controlType !== 'variable'">
                {{ $t("WebInterviewUI.Interview_Overview_NotAnswered") }}
            </div>
            <div class="btn-link" v-if="item.state == 'Unanswered' && item.controlType === 'variable'">
                {{ $t("WebInterviewUI.Interview_Overview_NoValue") }}
            </div>
        </div>

        <AdditionalInfo ref="additionalInfo" :item="item" :addCommentsAllowed="$store.getters.addCommentsAllowed" />
    </div>
</template>

<script>
const State = {
    Answered: 'Answered',
    Commented: 'Commented',
    Invalid: 'Invalid',
    Unanswered: 'Unanswered',
}
import AdditionalInfo from './OverviewItemAdditionalInfo'
import api from '~/shared/api'
import moment from 'moment'

export default {
    props: {
        item: {
            required: true,
            type: Object,
        },
    },

    data() {
        return {
            watcher: null,
        }
    },

    mounted() {
        if (this.item.isGroup || this.item.isSection) {
            this.$emit('mount', {
                el: this.$el,
                item: this.item,
            })
        }
    },

    destroyed() {
        if (this.watcher != null) {
            this.watcher.destroy()
        }
    },
    methods: {
        showAdditionalDetails() {
            if (this.item.isGroup || this.item.isSection)
                return

            const cantLeaveCommentAndNoWarningsNoErrors = !this.item.supportsComments
                && !this.item.hasWarnings
                && !this.item.hasErrors

            if (cantLeaveCommentAndNoWarningsNoErrors)
                return

            this.$emit('showAdditionalInfo', this)
            this.$refs.additionalInfo.show()
        },
        hideAdditionalDetails() {
            if (this.$refs.additionalInfo)
                this.$refs.additionalInfo.close()
        },
        parseGps(str) {
            return JSON.parse(str || '{ latitude: 0, longitude: 0 }')
        },
    },
    computed: {
        interviewId() {
            return this.$route.params.interviewId
        },
        areaAnswerUrl() {
            return `${this.$store.getters.basePath}Interview/InterviewAreaFrame/${this.interviewId}?questionId=${this.item.id}`
        },
        gpsAnswer() {
            return this.parseGps(this.item.answer)
        },
        goolgeMapUrl() {
            return `${this.$config.googleMapsBaseUrl}/maps?q=${this.gpsAnswer.latitude},${this.gpsAnswer.longitude}`
        },
        audioRecordPath() {
            return api.resources.audioRecordUri(this.interviewId, this.item.answer)
        },
        itemClass() {
            return {
                group: this.item.isGroup,
                section: this.item.isSection,
                unanswered: this.item.state == State.Unanswered,
                invalid: this.item.state == State.Invalid,
                hasComment: this.item.hasComment,
            }
        },
        hasDate() {
            if (!this.item.answerTimeUtc)
                return false
            if (this.item.isGroup || this.item.isSection)
                return false
            return true
        },
        answerDate() {
            if (!this.hasDate) return
            let local = moment.utc(this.item.answerTimeUtc).local()
            return local.format('MMM DD')
        },
        answerTime() {
            if (!this.hasDate) return
            let local = moment.utc(this.item.answerTimeUtc).local()
            return local.format('HH:mm')
        },
        attachmentContentId() {
            return this.item.attachmentContentId
        },
        hasAttachment() {
            return this.attachmentContentId != null
        },
    },
    components: {
        AdditionalInfo,
    },
}
</script>
