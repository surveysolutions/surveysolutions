<template>
    <div v-if="visible" :class="cssClass" @click="toggleSidebarPanel">
        <span v-if="showFoldbackButtonAsHamburger"></span>
        <template v-else>
            <span class="arrow"></span>
            <span class="arrow"></span>
            <span class="glyphicon glyphicon-tasks" aria-hidden="true"></span>
        </template>
    </div>
</template>
<script lang="js">
export default {
    name: 'wb-humburger',
    props: {
        showFoldbackButtonAsHamburger: {
            type: Boolean,
            default: true,
        },
        visible: {
            type: Boolean, default: true,
        },
    },
    computed: {
        cssClass() {
            if (this.showFoldbackButtonAsHamburger)
                return 'humburger-foldback-button'
            return 'foldback-button'
        },
    },
    methods: {
        toggleSidebarPanel() {
            this.$store.dispatch('toggleSidebarPanel')
        },
    },
}
</script>
