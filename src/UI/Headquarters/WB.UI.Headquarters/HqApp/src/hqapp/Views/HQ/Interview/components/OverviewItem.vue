<template>
    <div :class="itemClass" class="overview-item">
        <div class="date">
            <div v-if="hasDate">{{answerDate}}</div>
            <div v-if="hasDate">{{answerTime}}</div>
        </div>
        <div ref="itemContent" class="item-content" @click="showAdditionalDetails">
            <h4>
                <span v-html="item.Title"></span>
                <template v-if="item.rosterTitle != null"><span> - </span>
                <i v-if="item.rosterTitle != null" v-html="item.rosterTitle"></i>
                </template>
            </h4>
            <div class="answer" v-if="item.State != 3">
                <div v-if="item.ControlType === 'image'">
                    <wb-attachment :filename="item.Answer" :previewOnly="true"></wb-attachment>
                </div>
                <div v-else-if="item.ControlType === 'audio'">
                    <audio controls preload="auto" 
                        style="width:300px" 
                        :src="audioRecordPath">
                    </audio>
                </div>
                <div v-else-if="item.ControlType === 'area'">
                    <iframe width="100%" height="250px" frameBorder="0" :src="areaAnswerUrl"></iframe>
                </div>
                <div v-else-if="item.ControlType === 'map'">
                    <img v-bind:src="googleMapPosition" draggable="false" />
                </div>
                <div v-else>
                      {{item.Answer}}
                </div>
              
              
            </div>
            <div class="btn-link" v-if="item.State == 3">{{$t("WebInterviewUI.Interview_Overview_NotAnswered")}}</div>
        </div>

        <AdditionalInfo ref="additionalInfo" :item="item" />
    </div>
</template>

<script>
const State = {
    Answered: 0,
    Commented: 1,
    Invalid: 2,
    Unanswered: 3
};
import Vue from "vue";
import AdditionalInfo from './OverviewItemAdditionalInfo'
import api from "~/shared/api"

export default {
    props: {
        item: {
            required: true,
            type: Object
        }
    },

    data() {
        return {
            watcher: null
        };
    },

    mounted() {
        if (this.item.isGroup || this.item.isSection) {
            this.$emit("mount", {
                el: this.$el,
                item: this.item
            });
        }
    },

    destroyed() {
        if (this.watcher != null) {
            this.watcher.destroy();
        }
    },
    methods: {
        showAdditionalDetails(){
            if (this.item.isGroup || this.item.isSection) 
                return;
            
            const cantLeaveCommentAndNoWarningsNoErrors = !this.item.SupportsComments 
                && !this.item.HasWarnings 
                && !this.item.HasErrors;

            if (cantLeaveCommentAndNoWarningsNoErrors)
                return;

            this.$emit("showAdditionalInfo", this);
            this.$refs.additionalInfo.show();
        },
        hideAdditionalDetails(){
            if (this.$refs.additionalInfo)
                this.$refs.additionalInfo.close();
        },
        parseGps(str)
        {
            const regex = new RegExp('^(?<lat>[0-9\.-]*),(?<lon>[0-9\.-]*)\[.+\].*', 'gis');
            let matches = regex.exec(str);
            return {
                latitude: matches["groups"].lat,
                longitude: matches["groups"].lon,
            };            
        }
    },
    computed: {
        interviewId(){
            return this.$route.params.interviewId;
        },
        areaAnswerUrl() {
            return `${this.$store.getters.basePath}Interview/InterviewAreaFrame/${this.interviewId}?questionId=${this.item.Id}`
        },
        googleMapPosition() {
            let coords = this.parseGps(this.item.Answer);
            return `${this.$config.googleMapsApiBaseUrl}/maps/api/staticmap?center=${coords.latitude},${coords.longitude}`
                + `&zoom=14&scale=0&size=385x200&markers=color:blue|label:O|${coords.latitude},${coords.longitude}`
                + `&key=${this.$config.googleApiKey}`
        },
        audioRecordPath() {
            return api.resources.audioRecordUri(this.interviewId, this.item.Answer);
        },
        itemClass() {
            return {
                group: this.item.isGroup,
                section: this.item.isSection,
                unanswered: this.item.State == State.Unanswered,
                invalid: this.item.State == State.Invalid,
                hasComment: this.item.HasComment
            };
        },
        hasDate(){
            if (!this.item.AnswerTimeUtc)
                return false;
            if  (this.item.isGroup || this.item.isSection)
                return false;
            return true;
        },
        answerDate(){
            if (!this.hasDate) return;   
            let local = moment.utc(this.item.AnswerTimeUtc).local();
            return local.format("MMM DD");
        },
        answerTime(){
            if (!this.hasDate) return;
            let local = moment.utc(this.item.AnswerTimeUtc).local();
            return local.format("HH:mm");
        }
    },
    components: {
        AdditionalInfo
    }
};
</script>
