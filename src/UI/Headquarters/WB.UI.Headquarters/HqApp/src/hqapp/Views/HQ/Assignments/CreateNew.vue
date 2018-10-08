<template>
        <main class="web-interview web-interview-for-supervisor" >
            <div class="container-fluid">
                <div class="row">
                      <div id="questionsList" class="unit-section" :class="sectionClass">
        
                   <component v-for="entity in entities" :key="entity.identity" :is="entity.entityType" :id="entity.identity"></component>
                      </div>
                </div>
            </div>
            <IdleTimeoutService />
        </main>
</template>

<script>
import Vue from "vue";

export default {
    computed: {},

    methods: {
        onResize() {
            var screenWidth = document.documentElement.clientWidth;
            this.$store.dispatch("screenWidthChanged", screenWidth);
        }
    },

    beforeMount() {
        return Vue.$api
            .hub({
                interviewId: window.CONFIG.model.id,
                review: false
            })
            .then(() => this.$store.dispatch("loadTakeNew"));
    },

    mounted() {
        const self = this;

        this.$nextTick(function() {
            window.addEventListener("resize", self.onResize);
            self.onResize();
        });
    },

    updated() {
        Vue.nextTick(() => {
            window.ajustNoticeHeight();
            window.ajustDetailsPanelHeight();
        });
    },
    components: {},

    beforeDestroy() {
        window.removeEventListener("resize", this.onResize);
    }
};
</script>
