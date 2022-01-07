<template>
    <div id="root">
        <header>
            <slot name="header"></slot>
        </header>
        <main class="web-interview"
            :class="{'fullscreen-hidden-content': sidebarHidden}">
            <div class="container-fluid">
                <slot></slot>
            </div>
        </main>
        <footer :class="{'footer-adaptive-content': sidebarHidden}">
            <slot name="footer"></slot>
        </footer>
        <span id="loadingPixel"
            style="display:none"
            :data-loading="isLoading"></span>
    </div>
</template>
<script lang="js">
import modal from '@/shared/modal'
export default {
    name: 'wb-layout',
    computed: {
        sidebarHidden() {
            return this.$store.state.webinterview.sidebar.sidebarHidden
        },
        isLoading() {
            return this.$store.getters.loadingProgress === true ? 'true' : 'false'
        },
    },
    mounted() {
        if(this.$config.loadedMessage) {
            modal.dialog(
                {
                    message: '<p>' + this.$config.loadedMessage + '</p>',
                    showCancelButton: false,
                }
            )
        }
    },
}

</script>
