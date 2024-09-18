<template>
    <router-view></router-view>
</template>

<script>
import { onMounted } from 'vue';

export default {
    name: 'App',
    setup() {
        //TODO: Migration
        //fix layout instead of recalculation
        onMounted(() => {
            window.ajustDetailsPanelHeight()
        });
    },
};
</script>
