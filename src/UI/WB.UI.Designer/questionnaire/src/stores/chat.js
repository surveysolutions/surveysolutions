import { defineStore } from 'pinia';
import { ref } from 'vue';

export const useChatStore = defineStore('chat', () => {
    const isOpen = ref(false);
    const questionnaireId = ref(null);
    const entityId = ref(null);
    const area = ref(null);

    function open(config = {}) {
        questionnaireId.value = config.questionnaireId || null;
        entityId.value = config.entityId || null;
        area.value = config.area || null;
        isOpen.value = true;
    }

    function close() {
        isOpen.value = false;
    }

    return {
        isOpen,
        questionnaireId,
        entityId,
        area,
        open,
        close
    };
});
