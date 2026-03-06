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
    </v-card>
</template>

<script>
import { ref, nextTick, watch } from 'vue';
import { useChatStore } from '../../../stores/chat';
import { useAssistant } from '../../../composables/assistant';
import { useTreeStore } from '../../../stores/tree';
import hljs from 'highlight.js/lib/core';
import csharp from 'highlight.js/lib/languages/csharp';
import DOMPurify from 'dompurify';
hljs.registerLanguage('csharp', csharp);

export default {
    name: 'ChatPanel',
    setup() {
        const chatStore = useChatStore();
        const treeStore = useTreeStore();
        const messages = ref([]);
        const currentMessage = ref('');
        const isLoading = ref(false);
        const messagesContainer = ref(null);

        // Initialize Assistant
        const { sendMessage: sendToAssistant, sendReaction: sendAssistantReaction } = useAssistant();

        // Reset chat history when panel is opened
        watch(() => chatStore.isOpen, (newVal) => {
            if (newVal) {
                messages.value = [];
                currentMessage.value = '';
            }
        });

        const close = () => {
            chatStore.close();
        };

        const clearHistory = () => {
            messages.value = [];
            currentMessage.value = '';

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
            const variableNames = treeStore.getVariableNames.variableNamesTokens
                ? treeStore.getVariableNames.variableNamesTokens.split('|').filter(Boolean)
                : [];

            const highlightCode = (code, language) => {
                let highlighted;
                try {
                    highlighted = hljs.highlight(code.trim(), { language }).value;
                } catch {
                    highlighted = hljs.highlight(code.trim(), { language: 'csharp' }).value;
                }
                return wrapVariables(highlighted, variableNames);
            };

            // Wrap known questionnaire variable names in text nodes (not inside HTML tags)
            const wrapVariables = (html, variables) => {
                if (!variables.length) return html;
                const pattern = new RegExp(`\\b(${variables.map(v => v.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')).join('|')})\\b`, 'g');
                return html.replace(/>([^<]*)</g, (match, text) => {
                    return '>' + text.replace(pattern, '<span class="hljs-variable">$1</span>') + '<';
                });
            };

            // Extract fenced code blocks first
            const codeBlocks = [];
            let result = content.replace(/```(\w*)\n?([\s\S]*?)```/g, (_, lang, code) => {
                const language = lang || 'csharp';
                const highlighted = highlightCode(code, language);
                const idx = codeBlocks.length;
                codeBlocks.push(
                    `<pre class="chat-code-block"><code class="hljs language-${language}">${highlighted}</code></pre>`
                );
                return `CODEBLOCK${idx}PLACEHOLDER`;
            });

            // Extract inline code blocks
            const inlineBlocks = [];
            result = result.replace(/`([^`\n]+)`/g, (_, code) => {
                const highlighted = wrapVariables(
                    hljs.highlight(code, { language: 'csharp' }).value,
                    variableNames
                );
                const idx = inlineBlocks.length;
                inlineBlocks.push(`<code class="chat-code-inline hljs">${highlighted}</code>`);
                return `INLINEBLOCK${idx}PLACEHOLDER`;
            });

            // Sanitize plain text
            result = DOMPurify.sanitize(result, { ALLOWED_TAGS: [], KEEP_CONTENT: true });

            // Basic markdown
            result = result
                .replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>')
                .replace(/\*(.*?)\*/g, '<em>$1</em>')
                .replace(/\n/g, '<br>');

            // Restore code blocks
            inlineBlocks.forEach((block, i) => {
                result = result.replace(`INLINEBLOCK${i}PLACEHOLDER`, block);
            });
            codeBlocks.forEach((block, i) => {
                result = result.replace(`CODEBLOCK${i}PLACEHOLDER`, block);
            });

            return result;
        };

        const formatTime = (timestamp) => {
            return new Date(timestamp).toLocaleTimeString([], {
                hour: '2-digit',
                minute: '2-digit'
            });
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
                const response = await callAssistant(messageText, chatStore.questionnaireId, chatStore.entityId, chatStore.area);

                const assistantMessage = {
                    id: Date.now() + 1,
                    role: 'assistant',
                    content: response,
                    timestamp: Date.now(),
                    reaction: 0
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

        const setReaction = async (message, index, reactionValue) => {
            if (!message || message.role !== 'assistant' || message.isError) return;

            const previous = getMessageReaction(message);
            const next = previous === reactionValue ? 0 : reactionValue;

            message.reaction = next;
            message.isLiked = next === 1;
            message.isDisliked = next === -1;

            try {
                await sendAssistantReaction(chatStore.questionnaireId, {
                    entityId: chatStore.entityId,
                    area: chatStore.area,
                    clientMessageId: message.id,
                    clientTimestamp: message.timestamp,
                    prompt: findPromptForAssistantMessage(index),
                    assistantResponse: message.content,
                    reaction: next
                });
            } catch (error) {
                console.error('Error sending assistant reaction:', error);
                message.reaction = previous;
                message.isLiked = previous === 1;
                message.isDisliked = previous === -1;
            }
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
                area: area
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

.chat-code-block {
    margin: 8px 0;
    border-radius: 6px;
    overflow-x: auto;
    font-size: 12px;
    line-height: 1.5;
}

.chat-code-block code {
    display: block;
    padding: 12px 14px;
    white-space: pre;
    font-family: 'Consolas', 'Monaco', 'Courier New', monospace;
}

.chat-code-inline {
    padding: 1px 5px;
    border-radius: 3px;
    font-size: 12px;
    font-family: 'Consolas', 'Monaco', 'Courier New', monospace;
}

/* Ace github theme token colors */
:deep(.hljs) {
    background: #f6f8fa;
    color: #000;
    border-radius: 4px;
}

:deep(.hljs-keyword),
:deep(.hljs-selector-tag),
:deep(.hljs-literal.hljs-boolean) {
    font-weight: bold;
}

:deep(.hljs-string),
:deep(.hljs-attr) {
    color: #D14;
}

:deep(.hljs-number),
:deep(.hljs-literal) {
    color: #099;
}

:deep(.hljs-comment) {
    color: #998;
    font-style: italic;
}

:deep(.hljs-built_in),
:deep(.hljs-title.function_),
:deep(.hljs-variable.language_) {
    color: #0086B3;
}

:deep(.hljs-title.class_),
:deep(.hljs-type) {
    color: teal;
}

:deep(.hljs-variable) {
    color: teal;
    font-weight: 600;
}
</style>
