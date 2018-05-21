<template>
    <main>
        <div class="container-fluid">
            <div class="row">
                <slot name="filters" />
                <div :class="information">
                    <div class="page-header clearfix" v-if="hasHeader">
                      <slot name="headers">
                        <h1>{{title}}</h1>
                        <h3 v-if="subtitle">{{ subtitle }}</h3>
                      </slot>
                      <slot name="subtitle" />
                    </div>
                    <slot />
                </div>
            </div>
           
            <slot name="modals" />
            <ModalFrame id="pendingProgress" ref="pending" :title="$t('Common.Loading')" :canClose="false">
                <div class="progress progress-striped active" style="margin-bottom:0;">
                    <div class="progress-bar" style="width: 100%"></div>
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
            default() { return true; }
        },
        hasFilter: {
            type: Boolean,
            default() { return false; }
        }
    },
    watch: {
        showProgress: function (value) {
            if (value) {
                $(this.$refs.pending.$el).modal({
                    backdrop: 'static',
                    keyboard: false
                });
            }
            else {
                $(this.$refs.pending.$el).modal("hide")
            }
        }
    },
    computed: {
        information() {
            return {
                "main-information": this.hasFilter,
                "main-information-no-filter": !this.hasFilter
            }
        },
        showProgress() {
            return this.$store.state.progress.pendingProgress;
        }
    }
}
</script>
