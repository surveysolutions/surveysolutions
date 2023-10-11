<template>
    <div class="questionnaire-tree">
        <router-view name="tree"></router-view>
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
