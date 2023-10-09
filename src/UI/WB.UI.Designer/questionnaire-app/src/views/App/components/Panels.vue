<template>
    <div class="left">
        <router-view name="tree"></router-view>
    </div>
    <div class="right">
        <router-view />
    </div>
</template>

<script>
export default {
    name: 'Panels',
    props: {},
    data() {
        return {};
    }
};
</script>
