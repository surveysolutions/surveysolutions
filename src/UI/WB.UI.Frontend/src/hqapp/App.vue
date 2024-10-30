<template>
    <router-view></router-view>
</template>

<script>
import { onMounted } from 'vue';

export default {
    name: 'App',
    setup() {
        //TODO
        //fix layout instead of recalculation
        onMounted(() => {
            if (typeof window.ajustNoticeHeight === 'function') {
                window.ajustNoticeHeight()
            }
            if (typeof window.ajustDetailsPanelHeight === 'function') {
                window.ajustDetailsPanelHeight()
            }
        });
    },
};
</script>
