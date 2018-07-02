<template>
    <div :class="itemClass" class="overview-item">
        <div class="item-content">
            <h6>
                <span v-html="item.Title"></span>
                <template v-if="item.rosterTitle != null"><span> - </span>
                <i v-if="item.rosterTitle != null" v-html="item.rosterTitle"></i>
                </template>
            </h6>
            <p class="answer" v-if="item.State != 3">{{item.Answer}}</p>
            <p class="btn-link" v-else>{{$t("WebInterviewUI.Interview_Overview_NotAnswered")}}</p>
        </div>
    </div>
</template>

<script>
const State = {
    Answered: 0,
    Commented: 1,
    Invalid: 2,
    Unanswered: 3
};

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

    computed: {
        itemClass() {
            return {
                group: this.item.isGroup,
                section: this.item.isSection,
                unanswered: this.item.State == State.Unanswered,
                invalid: this.item.State == State.Invalid,
                hasComment: this.item.HasComment
            };
        }
    }
};
</script>
