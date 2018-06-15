<template>
    <div :class="itemClass" class="overview-item">
        <h6 v-html="item.Title"></h6>
        <p class="answer" v-if="item.State != 3">{{item.Answer}}</p>
        <p class="btn-link" v-else>{{$t("WebInterviewUI.Interview_Overview_NotAnswered")}}</p>
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
        if (this.item.isGroup) {
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
                group: this.item.IsGroup,
                section: this.item.IsSection,
                unanswered: this.item.State == State.Unanswered,
                invalid: this.item.Status == State.Invalid
            };
        }
    }
};
</script>

<style lang="css">
.overview-item {
    position: relative;
    padding: 10px 30px 10px 70px;
}

.overview-item.section {
    border-top: 2px solid #dbdfe2; /*$gray-form;*/
}

.overview-item.section h6 {
    font-size: 20px;
    font-weight: bolder;
    color: #b4b4b4;
    text-transform: uppercase;
}

.overview-item.group {
    border-top: 2px solid #dbdfe2; /*$gray-form;*/
}

.overview-item.group h6 {
    font-size: medium;
    color: #b4b4b4;
    text-transform: uppercase;
}

.overview-item h6 {
    font-size: 16px;
    font-weight: lighter;
    color: #212121;
}

.overview-item .answer {
    font-weight: bold;
    font-size: 14px;
    color: #212121;
}

.overview-item.unanswered {
    background-color: #e1e1e1;
}

.overview-item.invalid {
    background-color: #fcdad1;
    color: #c70000;
}
</style>
