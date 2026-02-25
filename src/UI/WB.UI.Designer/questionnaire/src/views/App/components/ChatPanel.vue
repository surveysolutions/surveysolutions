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
                                            <v-btn variant="text" size="x-small"
                                                :icon="getMessageReaction(message) === 1 ? 'mdi-thumb-up' : 'mdi-thumb-up-outline'"
                                                :color="getMessageReaction(message) === 1 ? 'primary' : undefined"
                                                @click="setReaction(message, index, 1)"
                                                :title="getMessageReaction(message) === 1 ? $t('Assistant.Unlike', 'Unlike') : $t('Assistant.Like', 'Like')" />
                                            <v-btn variant="text" size="x-small"
                                                :icon="getMessageReaction(message) === -1 ? 'mdi-thumb-down' : 'mdi-thumb-down-outline'"
                                                :color="getMessageReaction(message) === -1 ? 'error' : undefined"
                                                @click="setReaction(message, index, -1)"
                                                :title="getMessageReaction(message) === -1 ? $t('Assistant.Undislike', 'Remove dislike') : $t('Assistant.Dislike', 'Dislike')" />
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

        <v-dialog v-model="isDislikeDialogOpen" max-width="560">
            <v-card>
                <v-card-title class="text-h6">
                    {{ $t('Chat.DislikeCommentTitle', 'Add a comment') }}
                </v-card-title>
                <v-card-text>
                    <div class="text-body-2 mb-2">
                        {{ $t('Chat.DislikeCommentHint', 'What was wrong with this answer?') }}
                    </div>
                    <v-textarea v-model="dislikeComment" variant="outlined" density="comfortable" auto-grow
                        :placeholder="$t('Chat.DislikeCommentPlaceholder', 'Write a short comment (optional)')" />
                </v-card-text>
                <v-card-actions>
                    <v-spacer />
                    <v-btn variant="text" @click="cancelDislikeComment">
                        {{ $t('Chat.Cancel', 'Cancel') }}
                    </v-btn>
                    <v-btn color="primary" variant="text" @click="confirmDislikeComment">
                        {{ $t('Chat.Send', 'Send') }}
                    </v-btn>
                </v-card-actions>
            </v-card>
        </v-dialog>
    </v-card>
</template>

<script>
import { ref, nextTick, watch } from 'vue';
import { useChatStore } from '../../../stores/chat';
import { useAssistant } from '../../../composables/assistant';

export default {
    name: 'ChatPanel',
    setup() {
        const chatStore = useChatStore();
        const messages = ref([]);
        const currentMessage = ref('');
        const isLoading = ref(false);
        const messagesContainer = ref(null);

        const conversationId = ref(null);

        const isDislikeDialogOpen = ref(false);
        const dislikeComment = ref('');
        const pendingDislike = ref(null);

        // Initialize Assistant
        const { sendMessage: sendToAssistant, sendReaction: sendAssistantReaction } = useAssistant();

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
                    comment: next === -1 ? (comment || null) : null
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

            // If user is setting a negative reaction, ask for a comment first.
            if (next === -1 && previous !== -1) {
                pendingDislike.value = { message, index, previous, next };
                dislikeComment.value = '';
                isDislikeDialogOpen.value = true;
                return;
            }

            await applyReaction({ message, index, previous, next, comment: null });
        };

        const cancelDislikeComment = () => {
            pendingDislike.value = null;
            dislikeComment.value = '';
            isDislikeDialogOpen.value = false;
        };

        const confirmDislikeComment = async () => {
            const pending = pendingDislike.value;
            pendingDislike.value = null;
            isDislikeDialogOpen.value = false;

            if (!pending) {
                dislikeComment.value = '';
                return;
            }

            const comment = dislikeComment.value;
            dislikeComment.value = '';
            await applyReaction({ ...pending, comment });
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
            isDislikeDialogOpen,
            dislikeComment,
            cancelDislikeComment,
            confirmDislikeComment,
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
