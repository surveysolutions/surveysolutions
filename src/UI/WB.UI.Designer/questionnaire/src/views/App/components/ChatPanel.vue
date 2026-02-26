<template>
    <v-card class="chat-container" style="margin: 0; border-radius: 0;">
        <v-card-title class="d-flex justify-space-between align-center pa-4">
            <div class="d-flex align-center">
                <span>{{ $t('Assistant.Title', 'AI Assistant') }}</span>
            </div>
            <div class="d-flex align-center">
                <v-btn icon="mdi-delete-sweep" variant="text" size="medium" @click="clearHistory"
                    :disabled="messages.length === 0" :title="$t('Assistant.ClearHistory', 'Clear history')"
                    style="padding-right: 10px;" />
                <v-btn icon="mdi-close" variant="text" size="medium" @click="close" />
            </div>
        </v-card-title>

        <v-divider style="margin: 0;" />

        <!-- Chat Messages -->
        <v-card-text class="chat-messages pa-0" ref="messagesContainer"
            style="height: calc(100vh - 200px); overflow-y: auto;">
            <div class="pa-4">
                <div v-if="messages.length === 0" class="text-center text-grey-darken-1 mt-8">
                    <v-icon size="48" class="mb-4">mdi-chat-outline</v-icon>
                    <p>{{ $t('Assistant.WelcomeMessage', 'Start a conversation with the AI assistant') }}</p>
                </div>

                <div v-for="(message, index) in messages" :key="message.id" class="mb-4">
                    <div :class="[
                        'message-bubble',
                        message.role === 'user' ? 'user-message' : 'assistant-message'
                    ]">
                        <div class="d-flex align-start">
                            <div class="flex-grow-1">
                                <div class="message-content">
                                    <p class="mb-1" v-html="formatMessage(message.content)"></p>
                                    <div class="d-flex align-center justify-space-between">
                                        <div v-if="message.role === 'assistant' && !message.isError"
                                            class="d-flex align-center">
                                            <v-btn variant="text" size="x-small" icon class="reaction-btn reaction-like"
                                                :color="getMessageReaction(message) === 1 ? 'success' : undefined"
                                                @click="setReaction(message, index, 1)"
                                                :title="getMessageReaction(message) === 1 ? $t('Assistant.Unlike', 'Unlike') : $t('Assistant.Like', 'Like')">
                                                <v-icon :size="24">{{ getMessageReaction(message) === 1 ? 'mdi-thumb-up'
                                                    : 'mdi-thumb-up-outline' }}</v-icon>
                                            </v-btn>
                                            <v-btn variant="text" size="x-small" icon
                                                class="reaction-btn reaction-dislike"
                                                :color="getMessageReaction(message) === -1 ? 'error' : undefined"
                                                @click="setReaction(message, index, -1)"
                                                :title="getMessageReaction(message) === -1 ? $t('Assistant.Undislike', 'Remove dislike') : $t('Assistant.Dislike', 'Dislike')">
                                                <v-icon :size="24">{{ getMessageReaction(message) === -1 ?
                                                    'mdi-thumb-down' : 'mdi-thumb-down-outline' }}</v-icon>
                                            </v-btn>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>

                <!-- Typing Indicator -->
                <div v-if="isLoading" class="mb-4">
                    <div class="message-bubble assistant-message">
                        <div class="d-flex align-start">
                            <div class="flex-grow-1">
                                <div class="message-content">
                                    <div class="typing-indicator">
                                        <span></span>
                                        <span></span>
                                        <span></span>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </v-card-text>

        <v-divider style="margin: 0;" />

        <!-- Input Area -->
        <v-card-actions class="pa-4">
            <v-textarea v-model="currentMessage" :placeholder="$t('Assistant.TypeMessage', 'Type your message...')"
                variant="outlined" density="comfortable" hide-details @keyup.enter.prevent="handleEnter"
                :disabled="isLoading" class="flex-grow-1" maxlength="2000" rows="2">
                <!-- <template v-slot:append-inner>
                    <v-btn icon="mdi-send" variant="text" color="primary" size="medium" @click="sendMessage"
                        :disabled="!currentMessage.trim() || isLoading" />
                </template> -->
            </v-textarea>
        </v-card-actions>

    </v-card>
</template>

<script>
import { ref, nextTick, watch, getCurrentInstance } from 'vue';
import { useChatStore } from '../../../stores/chat';
import { useAssistant } from '../../../composables/assistant';

export default {
    name: 'ChatPanel',
    setup() {
        const vm = getCurrentInstance()?.proxy;
        const chatStore = useChatStore();
        const messages = ref([]);
        const currentMessage = ref('');
        const isLoading = ref(false);
        const messagesContainer = ref(null);

        const conversationId = ref(null);

        // Initialize Assistant
        const { sendMessage: sendToAssistant, sendReaction: sendAssistantReaction } = useAssistant();

        const promptUnlikeComment = () => {
            return new Promise(resolve => {
                const confirmPrompt = vm?.$confirmPrompt;
                if (typeof confirmPrompt !== 'function') {
                    resolve({ confirmed: true, comment: '' });
                    return;
                }

                confirmPrompt({
                    header: vm?.$t?.('Assistant.Unlike', 'Unlike') || 'Unlike',
                    title: vm?.$t?.('Assistant.UnlikeCommentTitle', 'Add a comment') || 'Add a comment',
                    okButtonTitle: vm?.$t?.('QuestionnaireEditor.OK', 'OK') || 'OK',
                    cancelButtonTitle: vm?.$t?.('QuestionnaireEditor.Cancel', 'Cancel') || 'Cancel',
                    inputPlaceholder: vm?.$t?.('Assistant.UnlikeCommentPlaceholder', 'Write a short comment (optional)')
                        || 'Write a short comment (optional)',
                    callback: (confirmed, value) => {
                        const comment = typeof value === 'string' ? value : '';
                        resolve({ confirmed: !!confirmed, comment });
                    }
                });
            });
        };

        const promptDislikeComment = () => {
            return new Promise(resolve => {
                const confirmPrompt = vm?.$confirmPrompt;
                if (typeof confirmPrompt !== 'function') {
                    resolve({ confirmed: true, comment: '' });
                    return;
                }

                confirmPrompt({
                    header: vm?.$t?.('Assistant.Unhelpful', 'Unhelpful') || 'Unhelpful',
                    title: vm?.$t?.('Assistant.DislikeCommentTitle', 'Add a comment') || 'Add a comment',
                    okButtonTitle: vm?.$t?.('Assistant.Send', 'Send') || 'Send',
                    cancelButtonTitle: vm?.$t?.('QuestionnaireEditor.Cancel', 'Cancel') || 'Cancel',
                    inputPlaceholder: vm?.$t?.('Assistant.DislikeCommentPlaceholder', 'Write a short comment (optional)')
                        || 'Write a short comment (optional)',
                    inputHint: vm?.$t?.('Assistant.DislikeCommentHint', 'What was wrong with this answer?')
                        || 'What was wrong with this answer?',
                    callback: (confirmed, value) => {
                        const comment = typeof value === 'string' ? value : '';
                        resolve({ confirmed: !!confirmed, comment });
                    }
                });
            });
        };

        // Reset chat history when panel is opened
        watch(() => chatStore.isOpen, (newVal) => {
            if (newVal) {
                messages.value = [];
                currentMessage.value = '';
                conversationId.value = null;
            }
        });

        const close = () => {
            chatStore.close();
        };

        const clearHistory = () => {
            messages.value = [];
            currentMessage.value = '';
            conversationId.value = null;

            if (typeof chatStore.clearHistory === 'function') {
                chatStore.clearHistory();
            }
        };

        const scrollToBottom = async () => {
            await nextTick();
            if (messagesContainer.value) {
                const element = messagesContainer.value.$el || messagesContainer.value;
                element.scrollTop = element.scrollHeight;
            }
        };

        const formatMessage = (content) => {
            // Simple formatting for line breaks and basic markdown
            return content
                .replace(/\n/g, '<br>')
                .replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>')
                .replace(/\*(.*?)\*/g, '<em>$1</em>');
        };

        const formatTime = (timestamp) => {
            return new Date(timestamp).toLocaleTimeString([], {
                hour: '2-digit',
                minute: '2-digit'
            });
        };

        const extractAssistantCallId = (meta) => {
            const raw = meta?.callLogId ?? meta?.CallLogId ?? null;
            const numeric = raw != null ? Number(raw) : null;
            return Number.isFinite(numeric) && numeric > 0 ? numeric : null;
        };

        const sendMessage = async () => {
            if (!currentMessage.value.trim()) return;

            // Add user message
            const userMessage = {
                id: Date.now(),
                role: 'user',
                content: currentMessage.value,
                timestamp: Date.now()
            };

            messages.value.push(userMessage);

            const messageText = currentMessage.value;
            currentMessage.value = '';
            isLoading.value = true;

            await scrollToBottom();

            try {
                // Call Assistant with conversation history
                const assistantResult = await callAssistant(messageText, chatStore.questionnaireId, chatStore.entityId, chatStore.area);
                const responseText = typeof assistantResult === 'string' ? assistantResult : assistantResult?.text;
                const responseMeta = typeof assistantResult === 'object' ? assistantResult?.meta : null;
                const nextConversationId = typeof assistantResult === 'object' ? assistantResult?.conversationId : null;
                if (nextConversationId) conversationId.value = nextConversationId;
                const assistantCallId = extractAssistantCallId(responseMeta);

                const assistantMessage = {
                    id: Date.now() + 1,
                    role: 'assistant',
                    content: responseText,
                    timestamp: Date.now(),
                    reaction: 0,
                    assistantCallId: assistantCallId
                };

                messages.value.push(assistantMessage);
                await scrollToBottom();
            } catch (error) {
                console.error('Error sending message:', error);

                const errorMessage = {
                    id: Date.now() + 1,
                    role: 'assistant',
                    content: error.message || 'Sorry, I encountered an error. Please try again.',
                    timestamp: Date.now(),
                    isError: true,
                    reaction: 0
                };

                messages.value.push(errorMessage);
                await scrollToBottom();
            } finally {
                isLoading.value = false;
            }
        };

        const handleEnter = (event) => {
            if (event.shiftKey) {
                // Allow default behavior for Shift+Enter (new line)
                return;
            }
            // Prevent default and send message on Enter
            event.preventDefault();
            sendMessage();
        };

        const findPromptForAssistantMessage = (assistantIndex) => {
            for (let i = assistantIndex - 1; i >= 0; i--) {
                const msg = messages.value[i];
                if (msg && msg.role === 'user') return msg.content || '';
            }
            return '';
        };

        const getMessageReaction = (message) => {
            if (!message) return 0;
            if (typeof message.reaction === 'number') return message.reaction;
            if (message.isLiked === true) return 1;
            if (message.isDisliked === true) return -1;
            return 0;
        };

        const applyReaction = async ({ message, index, previous, next, comment }) => {
            message.reaction = next;
            message.isLiked = next === 1;
            message.isDisliked = next === -1;

            try {
                const assistantCallId = message.assistantCallId;
                if (!assistantCallId) throw new Error('assistantCallId is missing');

                const providerReaction = next === 1 ? 1 : next === -1 ? 2 : 0;
                await sendAssistantReaction(chatStore.questionnaireId, {
                    entityId: chatStore.entityId,
                    clientMessageId: message.id,
                    clientTimestamp: message.timestamp,
                    prompt: findPromptForAssistantMessage(index),
                    assistantResponse: message.content,
                    assistantCallId: assistantCallId,
                    reaction: providerReaction,
                    comment: comment || null
                });
            } catch (error) {
                console.error('Error sending assistant reaction:', error);
                message.reaction = previous;
                message.isLiked = previous === 1;
                message.isDisliked = previous === -1;
            }
        };

        const setReaction = async (message, index, reactionValue) => {
            if (!message || message.role !== 'assistant' || message.isError) return;

            const previous = getMessageReaction(message);
            const next = previous === reactionValue ? 0 : reactionValue;

            // If user removes a positive reaction, ask for a comment via $confirm.
            if (previous === 1 && next === 0 && reactionValue === 1) {
                const { confirmed, comment } = await promptUnlikeComment();
                if (!confirmed) return;
                await applyReaction({ message, index, previous, next, comment });
                return;
            }

            // If user sets a negative reaction, ask for a comment in confirm-styled prompt.
            if (next === -1 && previous !== -1) {
                const { confirmed, comment } = await promptDislikeComment();
                if (!confirmed) return;
                await applyReaction({ message, index, previous, next, comment });
                return;
            }

            await applyReaction({ message, index, previous, next, comment: null });
        };

        const callAssistant = async (userMessage, questionnaireId, entityId, area) => {
            const conversationHistory = [];

            // Add previous messages from the current conversation (exclude error messages)
            messages.value.forEach(msg => {
                if (!msg.isError) {
                    conversationHistory.push({
                        role: msg.role,
                        content: msg.content
                    });
                }
            });

            // Call Assistant API with userMessage as a separate parameter
            return await sendToAssistant(userMessage, conversationHistory, {
                questionnaireId: questionnaireId,
                entityId: entityId,
                area: area,
                conversationId: conversationId.value
            });
        };

        return {
            clearHistory,
            messages,
            currentMessage,
            isLoading,
            messagesContainer,
            close,
            sendMessage,
            handleEnter,
            getMessageReaction,
            setReaction,
            formatMessage,
            formatTime
        };
    }
};
</script>

<style scoped>
.chat-container {
    display: flex;
    flex-direction: column;
    font-size: 14px;
    height: 100%;
    width: 100%;
}

.chat-container .v-card-title {
    display: flex !important;
    justify-content: space-between !important;
    font-size: 16px;
}

.chat-container :deep(.v-field--appended) {
    padding-inline-end: 0px !important;
}

.chat-messages {
    flex: 1;
    overflow-y: auto;
    font-size: 14px;
}

.message-bubble {
    margin-bottom: 16px;
}

.message-bubble.user-message {
    justify-content: flex-end;
    display: flex;
}

.message-bubble.assistant-message {
    justify-content: flex-start;
}

.message-bubble p {
    font-size: 14px;
    margin: 0;
}

.user-message .message-content {
    background-color: rgb(var(--v-theme-primary));
    color: white;
    padding: 12px 16px;
    border-radius: 18px;
    border-top-right-radius: 4px;
    font-size: 14px;
}

.assistant-message .message-content {
    background-color: rgb(var(--v-theme-surface-variant));
    color: rgb(var(--v-theme-on-surface-variant));
    padding: 12px 16px;
    border-radius: 18px;
    font-size: 14px;
}

.typing-indicator {
    display: flex;
    align-items: center;
    gap: 4px;
    padding: 8px 0;
}

.typing-indicator span {
    height: 8px;
    width: 8px;
    background-color: rgb(var(--v-theme-on-surface-variant));
    border-radius: 50%;
    display: inline-block;
    animation: typing 1.4s infinite ease-in-out;
}

.typing-indicator span:nth-child(1) {
    animation-delay: -0.32s;
}

.typing-indicator span:nth-child(2) {
    animation-delay: -0.16s;
}

.v-card-actions {
    padding-bottom: 10px !important;
}

/* Remove Vuetify "text button" hover/focus overlay for like/dislike buttons. */
.chat-container :deep(.reaction-btn:hover .v-btn__overlay),
.chat-container :deep(.reaction-btn:focus-visible .v-btn__overlay),
.chat-container :deep(.reaction-btn:hover .v-btn__underlay),
.chat-container :deep(.reaction-btn:focus-visible .v-btn__underlay) {
    opacity: 0 !important;
}

.chat-container :deep(.reaction-btn:hover) {
    background-color: transparent !important;
}

.chat-container :deep(.reaction-like:hover .v-icon),
.chat-container :deep(.reaction-like:focus-visible .v-icon) {
    color: rgb(var(--v-theme-success)) !important;
}

.chat-container :deep(.reaction-dislike:hover .v-icon),
.chat-container :deep(.reaction-dislike:focus-visible .v-icon) {
    color: rgb(var(--v-theme-error)) !important;
}

@keyframes typing {

    0%,
    80%,
    100% {
        transform: scale(0.8);
        opacity: 0.5;
    }

    40% {
        transform: scale(1);
        opacity: 1;
    }
}
</style>
