<template>
    <div class="panels-layout">
        <div class="questionnaire-tree" :style="{ '--panel-offset': isOpen ? panelWidth + 'px' : '0px' }">
            <router-view name="tree"></router-view>
            <router-view />
        </div>
        <div v-if="isOpen" id="resizablePanel" ref="resizablePanel" :style="{ width: panelWidth + 'px' }">
            <div class="resize-handle" @mousedown="startResize"></div>
            <ChatPanel class="panel-content" />
        </div>
    </div>
</template>

<script>
import { ref, onBeforeUnmount } from 'vue';
import { storeToRefs } from 'pinia';
import { useChatStore } from '../../../stores/chat';
import ChatPanel from './ChatPanel.vue';
import { getUiDirection } from '../../../plugins/uiLocale';

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
        const isRtl = getUiDirection() === 'rtl';

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
            const deltaX = isRtl
                ? e.clientX - startX.value
                : startX.value - e.clientX;
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
.panels-layout {
    position: relative;
    height: 100%;
    width: 100%;
}

.questionnaire-tree {
    position: absolute;
    top: 0;
    inset-inline-start: 0;
    inset-inline-end: var(--panel-offset, 0);
    bottom: 0;
}

#resizablePanel {
    position: absolute;
    top: 0;
    inset-inline-end: 0;
    bottom: 0;
    background: white;
    border-inline-start: 1px solid #ddd;
    overflow: hidden;
    display: flex;
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
