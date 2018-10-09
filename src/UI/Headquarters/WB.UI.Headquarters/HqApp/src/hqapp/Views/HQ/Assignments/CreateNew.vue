<template>
        <main class="web-interview web-interview-for-supervisor" >
            <div class="container-fluid">
                <div class="row">
                    <div class="unit-section complete-section">
                        <div class="wrapper-info error">
                            <div class="container-info">
                                <h2> {{ questionnaireTitle }} </h2>
                            </div>
                        </div>
                        <component v-for="entity in entities" :key="entity.identity" :is="entity.entityType" :id="entity.identity" fetchOnMount></component>
                    </div>
                </div>
            </div>
            <IdleTimeoutService />
        </main>
</template>

<script>
import Vue from "vue";

export default {
    computed: {
        entities() {
            return this.$store.state.takeNew.takeNew.entities
        },
        questionnaireTitle(){
            return this.$store.state.takeNew.takeNew.interview.questionnaireTitle
        }
    },

    methods: {
        onResize() {
            var screenWidth = document.documentElement.clientWidth
            this.$store.dispatch("screenWidthChanged", screenWidth)
        }
    },

    beforeMount() {
        return Vue.$api
            .hub({
                interviewId: window.CONFIG.model.id,
                review: false
            })
            .then(() => this.$store.dispatch("loadTakeNew"))
    },

    mounted() {
        const self = this;

        this.$nextTick(function() {
            window.addEventListener("resize", self.onResize)
            self.onResize()
        });
    },

    updated() {
        Vue.nextTick(() => {
            window.ajustNoticeHeight()
            window.ajustDetailsPanelHeight()
        });
    },
    components: {},

    beforeDestroy() {
        window.removeEventListener("resize", this.onResize)
    }
};
</script>
