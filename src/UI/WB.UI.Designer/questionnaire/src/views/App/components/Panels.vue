<template>
    <div class="questionnaire-tree" :class="{ 'col-xs-10': chatStore.isOpen }">
        <router-view name="tree"></router-view>
        <router-view />
    </div>
    <div v-if="chatStore.isOpen">
        <div class="assistant-panel">
            <ChatPanel v-if="chatStore.isOpen" />
        </div>
    </div>

</template>

<script>
import { useChatStore } from '../../../stores/chat';
import ChatPanel from './ChatPanel.vue';

export default {
    name: 'Panels',
    components: {
        ChatPanel
    },
    props: {},
    setup() {
        const chatStore = useChatStore();
        return {
            chatStore
        };
    },
    data() {
        return {};
    }
};
</script>
