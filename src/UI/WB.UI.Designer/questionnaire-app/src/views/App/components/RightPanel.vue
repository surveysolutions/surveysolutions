<template>
    <div
        class="question-editor col-xs-6"
        v-class="{ commenting: isCommentsBlockVisible }"
        ui-view
    >
        <router-view />
    </div>

    <div class="comments-editor col-xs-6" ui-view="comments"></div>
</template>

<script>
export default {
    name: 'RightPanel',
    props: {},
    data() {
        return {};
    }
};
</script>
