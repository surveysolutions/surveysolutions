<template>
    <div class="questionnaire-tree" :style="{ marginRight: isOpen ? panelWidth + 'px' : '0px' }">
        <router-view name="tree"></router-view>
        <router-view />
    </div>
    <div v-if="isOpen" id="resizablePanel" ref="resizablePanel" :style="{ width: panelWidth + 'px' }">
        <div class="resize-handle" @mousedown="startResize"></div>
        <ChatPanel class="panel-content" />
    </div>

</template>

<script>
import { ref, onMounted, onBeforeUnmount } from 'vue';
import { storeToRefs } from 'pinia';
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
        const { isOpen } = storeToRefs(chatStore);
        const resizablePanel = ref(null);
        const panelWidth = ref(300); // Default width
        const isResizing = ref(false);
        const startX = ref(0);
        const startWidth = ref(0);

        const startResize = (e) => {
            isResizing.value = true;
            startX.value = e.clientX;
            startWidth.value = panelWidth.value;
            document.addEventListener('mousemove', resize);
            document.addEventListener('mouseup', stopResize);
            e.preventDefault();
        };

        const resize = (e) => {
            if (!isResizing.value) return;
            const deltaX = startX.value - e.clientX; // Subtract because panel grows from right to left
            const newWidth = startWidth.value + deltaX;
            // Set min and max width constraints
            panelWidth.value = Math.max(200, Math.min(newWidth, 600));
        };

        const stopResize = () => {
            isResizing.value = false;
            document.removeEventListener('mousemove', resize);
            document.removeEventListener('mouseup', stopResize);
        };

        onBeforeUnmount(() => {
            document.removeEventListener('mousemove', resize);
            document.removeEventListener('mouseup', stopResize);
        });

        return {
            chatStore,
            isOpen,
            resizablePanel,
            panelWidth,
            startResize
        };
    },
    data() {
        return {};
    }
};
</script>

<style scoped>
#resizablePanel {
    position: fixed;
    right: 0;

    bottom: 0;
    background: white;
    border-left: 1px solid #ddd;
    overflow: hidden;
    z-index: 100;
    display: flex;
    height: calc(100% - (75px + 2px));
}

.resize-handle {
    width: 5px;
    background: #e0e0e0;
    cursor: ew-resize;
    transition: background 0.2s;
    flex-shrink: 0;
}

.resize-handle:hover {
    background: #999;
}

.resize-handle:active {
    background: #666;
}

.panel-content {
    flex: 1;
    overflow: auto;
}
</style>
