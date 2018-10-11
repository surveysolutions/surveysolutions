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
            <p class="answer" v-if="item.State != 3">{{item.Answer}}</p>
            <p class="btn-link" v-else>{{$t("WebInterviewUI.Interview_Overview_NotAnswered")}}</p>
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
        }
    },
    computed: {
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
