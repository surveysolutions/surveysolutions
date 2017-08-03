<template>
    <div id="root">
        <header>
            <slot name="header"></slot>
        </header>
        <main class="web-interview" :class="{'fullscreen-hidden-content': sidebarHidden}">
            <div class="container-fluid">
                <slot></slot>
            </div>
        </main>
        <footer :class="{'footer-adaptive-content': sidebarHidden}">
            <slot name="footer"></slot>
        </footer>
    </div>
</template>
<script lang="js">
    export default {
        name: 'wb-layout',
        computed: {
            sidebarHidden() {
                return this.$store.state.sidebar.sidebarHidden
            }
        }
    }

</script>
