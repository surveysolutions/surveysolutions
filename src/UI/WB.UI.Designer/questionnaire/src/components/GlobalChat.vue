<template>
    <Teleport to="body">
        <ChatDialog v-model="chatStore.isOpen" :questionnaireId="chatStore.questionnaireId"
            :entityId="chatStore.entityId" :area="chatStore.area" />
    </Teleport>
</template>

<script>
import { useChatStore } from '../stores/chat';
import ChatDialog from '../views/App/components/ChatDialog.vue';

export default {
    name: 'GlobalChat',
    components: {
        ChatDialog
    },
    setup() {
        const chatStore = useChatStore();
        return { chatStore };
    }
};
</script>
