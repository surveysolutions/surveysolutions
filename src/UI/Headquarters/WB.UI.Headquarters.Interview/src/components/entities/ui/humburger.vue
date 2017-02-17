<template>
    <div class="humburger-foldback-button" @click="toggleSidebarPanel">
        <span></span>
    </div>
</template>
<script lang="ts">
    export default {
        name: "wb-humburger",
        methods: {
            toggleSidebarPanel() {
                this.$store.dispatch("toggleSidebarPanel")
            }
        }
    }
</script>
