<template>
    <div class="question-editor col-xs-6" :class="{ commenting: isCommentsBlockVisible }" ui-view>
        <router-view />
    </div>

    <div class="comments-editor col-xs-6" ui-view="comments">
        <router-view name="comments"></router-view>>
    </div>
</template>

<script>
import { useCommentsStore } from '../../../stores/comments';

export default {
    name: 'RightPanel',
    props: {},
    data() {
        return {};
    },
    setup() {
        const commentsStore = useCommentsStore();

        return {
            commentsStore
        };
    },
    computed: {
        isCommentsBlockVisible() {
            return this.commentsStore.getIsCommentsBlockVisible;
        }
    },
};
</script>
