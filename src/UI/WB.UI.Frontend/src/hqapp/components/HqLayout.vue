<template>
    <main :class="mainClass"
        :data-suso="tag">
        <div :class="{ 'container' : fixedWidth, 'container-fluid': !fixedWidth }" >
            <div :class="{ 'row' : hasRow }">
                <slot name="filters" />
                <div :class="information">
                    <div class="page-header clearfix"
                        v-if="hasHeader">
                        <div :class="{'neighbor-block-to-search': hasSearch}">
                            <slot name="headers">
                                <div :class="{'topic-with-button': topicButtonRef}">
                                    <h1 v-html='title'></h1>
                                    <a v-if="topicButtonRef"
                                        class="btn btn-success"
                                        :href="topicButtonRef">
                                        {{ topicButton }}
                                    </a>
                                </div>
                                <i v-if="subtitle"
                                    v-html="subtitle">
                                </i>
                            </slot>
                            <slot name="subtitle" />
                        </div>
                    </div>
                    <slot />
                </div>
            </div>
            <slot name="modals" />
            <ModalFrame id="pendingProgress"
                ref="pending"
                :title="$t('Common.Loading')"
                :canClose="false">
                <div class="progress progress-striped active"
                    style="margin-bottom:0;">
                    <div class="progress-bar"
                        style="width: 100%"></div>
                </div>
            </ModalFrame>
        </div>
    </main>
</template>

<script>
export default {
    props: {
        title: String,
        subtitle: String,
        hasHeader: {
            type: Boolean,
            default() { return true },
        },
        tag: String,
        hasFilter: {
            type: Boolean,
            default() { return false },
        },
        hasSearch: {
            type: Boolean,
            default() { return false },
        },
        fixedWidth: {
            type: Boolean,
            default() { return false },
        },
        hasRow: {
            type: Boolean,
            default: true,
        },
        topicButton: String,
        topicButtonRef: String,
        mainClass: String,
    },
    watch: {
        showProgress: function (value) {
            if (value) {
                $(this.$refs.pending.$el).modal({
                    backdrop: 'static',
                    keyboard: false,
                })
            }
            else {
                $(this.$refs.pending.$el).modal('hide')
            }
        },
    },
    computed: {
        information() {
            return {
                'main-information': this.hasFilter,
                'main-information-no-filter': !this.hasFilter,
            }
        },
        showProgress() {
            return this.$store.state.progress.pendingProgress
        },
    },
}
</script>
