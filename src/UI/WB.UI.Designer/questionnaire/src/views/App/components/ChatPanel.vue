<template>
    <v-card class="chat-container" style="margin: 0; border-radius: 0;">
        <v-card-title class="d-flex justify-space-between align-center pa-4">
            <div class="d-flex align-center">
                <span>{{ $t('Assistant.Title') }}</span>
            </div>
            <div class="d-flex align-center">
                <v-btn icon="mdi-delete-sweep" variant="text" size="medium" class="header-action-btn header-clear"
                    @click="clearHistory" :disabled="messages.length === 0" :title="$t('Assistant.ClearHistory')"
                    style="padding-right: 10px;" />
                <v-btn icon="mdi-close" variant="text" size="medium" class="header-action-btn header-close"
                    @click="close" />
            </div>
        </v-card-title>

        <v-divider style="margin: 0;" />

        <!-- Chat Messages -->
        <v-card-text class="chat-messages pa-0" ref="messagesContainer" @click="handleCodeCopy"
            style="height: calc(100vh - 200px); overflow-y: auto;">
            <div class="pa-4">
                <div v-if="messages.length === 0" class="text-center text-grey-darken-1 mt-8">
                    <v-icon size="48" class="mb-4">mdi-chat-outline</v-icon>
                    <p>{{ $t('Assistant.WelcomeMessage') }}</p>
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
                                        <div v-if="message.role === 'assistant' && !message.isError && !!message.assistantCallId"
                                            class="d-flex align-center">
                                            <v-btn variant="text" size="x-small" icon class="reaction-btn reaction-like"
                                                :color="getMessageReaction(message) === 1 ? 'success' : undefined"
                                                @click="setReaction(message, index, 1)"
                                                :title="getMessageReaction(message) === 1 ? $t('Assistant.Unlike') : $t('Assistant.Like')">
                                                <v-icon :size="24">{{ getMessageReaction(message) === 1 ? 'mdi-thumb-up'
                                                    : 'mdi-thumb-up-outline' }}</v-icon>
                                            </v-btn>
                                            <v-btn variant="text" size="x-small" icon
                                                class="reaction-btn reaction-dislike"
                                                :color="getMessageReaction(message) === -1 ? 'error' : undefined"
                                                @click="setReaction(message, index, -1)"
                                                :title="getMessageReaction(message) === -1 ? $t('Assistant.Undislike') : $t('Assistant.Dislike')">
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
            <v-textarea v-model="currentMessage" :placeholder="$t('Assistant.TypeMessage')" variant="outlined"
                density="comfortable" hide-details @keyup.enter.prevent="handleEnter" :disabled="isLoading"
                class="flex-grow-1" maxlength="2000" rows="2">
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
import { useTreeStore } from '../../../stores/tree';
import hljs from 'highlight.js/lib/core';
import csharp from 'highlight.js/lib/languages/csharp';
import DOMPurify from 'dompurify';
hljs.registerLanguage('csharp', csharp);

export default {
    name: 'ChatPanel',
    setup() {
        const vm = getCurrentInstance()?.proxy;
        const chatStore = useChatStore();
        const treeStore = useTreeStore();
        const messages = ref([]);
        const currentMessage = ref('');
        const isLoading = ref(false);
        const messagesContainer = ref(null);

        const conversationId = ref(null);

        // Initialize Assistant
        const { sendMessage: sendToAssistant, sendReaction: sendAssistantReaction } = useAssistant();

        const promptDislikeComment = () => {
            return new Promise(resolve => {
                const confirmPrompt = vm?.$confirmPrompt;
                if (typeof confirmPrompt !== 'function') {
                    resolve({ confirmed: false, comment: '' });
                    return;
                }

                confirmPrompt({
                    header: vm?.$t?.('Assistant.SendFeedback'),
                    title: vm?.$t?.('Assistant.DislikeCommentHint'),
                    okButtonTitle: vm?.$t?.('Assistant.Send'),
                    cancelButtonTitle: vm?.$t?.('QuestionnaireEditor.Cancel'),
                    inputPlaceholder: vm?.$t?.('Assistant.DislikeCommentPlaceholder'),
                    draggable: true,
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
            const variableNames = treeStore.getVariableNames.variableNamesTokens
                ? treeStore.getVariableNames.variableNamesTokens.split('|').filter(Boolean)
                : [];

            const highlightCode = (code, language) => {
                let highlighted;
                let actualLanguage = language;
                try {
                    highlighted = hljs.highlight(code.trim(), { language }).value;
                } catch {
                    actualLanguage = 'csharp';
                    highlighted = hljs.highlight(code.trim(), { language: 'csharp' }).value;
                }
                return { highlighted: wrapVariables(highlighted, variableNames), actualLanguage };
            };

            // Wrap known questionnaire variable names only in bare text nodes
            // (not inside already-classified hljs token spans), using DOM traversal.
            const wrapVariables = (html, variables) => {
                if (!variables.length) return html;
                const pattern = new RegExp(`\\b(${variables.map(v => v.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')).join('|')})\\b`, 'g');

                const container = document.createElement('div');
                container.innerHTML = html;

                const walker = document.createTreeWalker(container, NodeFilter.SHOW_TEXT);
                const textNodes = [];
                let node;
                while ((node = walker.nextNode())) {
                    // Only process bare text nodes — skip any already inside an hljs token span
                    const parentClass = node.parentElement?.className || '';
                    if (!parentClass.includes('hljs-')) {
                        textNodes.push(node);
                    }
                }

                for (const textNode of textNodes) {
                    const text = textNode.nodeValue;
                    pattern.lastIndex = 0;
                    if (!pattern.test(text)) continue;

                    pattern.lastIndex = 0;
                    const fragment = document.createDocumentFragment();
                    let lastIndex = 0;
                    let match;
                    while ((match = pattern.exec(text)) !== null) {
                        if (match.index > lastIndex) {
                            fragment.appendChild(document.createTextNode(text.slice(lastIndex, match.index)));
                        }
                        const span = document.createElement('span');
                        span.className = 'hljs-variable';
                        span.textContent = match[1];
                        fragment.appendChild(span);
                        lastIndex = match.index + match[0].length;
                    }
                    if (lastIndex < text.length) {
                        fragment.appendChild(document.createTextNode(text.slice(lastIndex)));
                    }
                    textNode.parentNode.replaceChild(fragment, textNode);
                }

                return container.innerHTML;
            };

            // Extract fenced code blocks first
            const nonce = Math.random().toString(36).slice(2);
            const codeBlocks = [];
            let result = content.replace(/```(\w*)\n?([\s\S]*?)```/g, (_, lang, code) => {
                const language = lang || 'csharp';
                const { highlighted, actualLanguage } = highlightCode(code, language);
                const idx = codeBlocks.length;
                const encoded = encodeURIComponent(code.trim());
                codeBlocks.push(
                    `<div class="chat-code-wrapper">` +
                    `<button class="chat-copy-btn" data-copy="${encoded}" title="Copy"><span class="mdi mdi-content-copy"></span></button>` +
                    `<pre class="chat-code-block"><code class="hljs language-${actualLanguage}">${highlighted}</code></pre>` +
                    `</div>`
                );
                return `\x00C${nonce}${idx}\x00`;
            });

            // Extract inline code blocks
            const inlineBlocks = [];
            result = result.replace(/`([^`\n]+)`/g, (_, code) => {
                const { highlighted } = highlightCode(code, 'csharp');
                const encoded = encodeURIComponent(code);
                const idx = inlineBlocks.length;
                inlineBlocks.push(
                    `<span class="chat-inline-wrapper">` +
                    `<code class="chat-code-inline hljs">${highlighted}</code>` +
                    `<button class="chat-copy-btn chat-copy-inline" data-copy="${encoded}" title="Copy"><span class="mdi mdi-content-copy"></span></button>` +
                    `</span>`
                );
                return `\x00I${nonce}${idx}\x00`;
            });

            // Sanitize plain text
            result = DOMPurify.sanitize(result, { ALLOWED_TAGS: [], KEEP_CONTENT: true });

            // Basic markdown
            result = result
                .replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>')
                .replace(/\*(.*?)\*/g, '<em>$1</em>')
                .replace(/\n/g, '<br>');

            // Restore code blocks using exact sentinel strings
            inlineBlocks.forEach((block, i) => {
                result = result.split(`\x00I${nonce}${i}\x00`).join(block);
            });
            codeBlocks.forEach((block, i) => {
                result = result.split(`\x00C${nonce}${i}\x00`).join(block);
            });

            return result;
        };

        const handleCodeCopy = (event) => {
            const btn = event.target.closest('.chat-copy-btn');
            if (!btn) return;
            const code = decodeURIComponent(btn.dataset.copy || '');
            navigator.clipboard.writeText(code).then(() => {
                const icon = btn.querySelector('.mdi');
                if (icon) {
                    icon.classList.replace('mdi-content-copy', 'mdi-check');
                    setTimeout(() => icon.classList.replace('mdi-check', 'mdi-content-copy'), 1500);
                }
            });
        };

        const formatTime = (timestamp) => {
            return new Date(timestamp).toLocaleTimeString([], {
                hour: '2-digit',
                minute: '2-digit'
            });
        };

        const extractAssistantCallId = (raw) => {
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
                const nextConversationId = typeof assistantResult === 'object' ? assistantResult?.conversationId : null;
                if (nextConversationId) conversationId.value = nextConversationId;
                const assistantCallId = extractAssistantCallId(assistantResult?.callLogId);

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

            if (next === -1 && previous !== -1) {
                // Send negative reaction immediately, then prompt for optional feedback.
                await applyReaction({ message, index, previous, next, comment: null });

                const { confirmed, comment } = await promptDislikeComment();
                if (confirmed) {
                    // Re-send same negative reaction with the user's comment (if any).
                    await applyReaction({ message, index, previous: -1, next: -1, comment });
                }
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
            formatTime,
            handleCodeCopy
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
    padding: 4px 0;
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

/* Make header action buttons behave like reaction buttons on hover/focus. */
.chat-container :deep(.header-action-btn:hover .v-btn__overlay),
.chat-container :deep(.header-action-btn:focus-visible .v-btn__overlay),
.chat-container :deep(.header-action-btn:hover .v-btn__underlay),
.chat-container :deep(.header-action-btn:focus-visible .v-btn__underlay) {
    opacity: 0 !important;
}

.chat-container :deep(.header-action-btn:hover) {
    background-color: transparent !important;
}

.chat-container :deep(.header-clear:hover .v-icon),
.chat-container :deep(.header-clear:focus-visible .v-icon) {
    color: rgb(var(--v-theme-error)) !important;
}

.chat-container :deep(.header-close:hover .v-icon),
.chat-container :deep(.header-close:focus-visible .v-icon) {
    color: rgb(var(--v-theme-error)) !important;
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

.chat-code-block {
    margin: 8px 0;
    border-radius: 6px;
    overflow-x: auto;
    font-size: 12px;
    line-height: 1.5;
}

:deep(.chat-code-wrapper) {
    position: relative;
}

:deep(.chat-inline-wrapper) {
    position: relative;
    display: inline-flex;
    align-items: center;
    gap: 2px;
}

:deep(.chat-copy-btn) {
    position: absolute;
    top: 5px;
    right: 5px;
    background: rgba(255, 255, 255, 0.9);
    border: 1px solid #d0d7de;
    border-radius: 4px;
    padding: 1px 4px;
    cursor: pointer;
    opacity: 0;
    transition: opacity 0.15s;
    line-height: 1;
    font-size: 11px;
    color: #57606a;
}

:deep(.chat-copy-inline) {
    position: static;
    background: transparent;
    border: none;
    padding: 0;
    font-size: 10px;
    color: #57606a;
    opacity: 0;
    transition: opacity 0.15s;
}

:deep(.chat-inline-wrapper:hover .chat-copy-inline) {
    opacity: 0.7;
}

:deep(.chat-copy-inline:hover) {
    opacity: 1 !important;
}

:deep(.chat-code-wrapper:hover .chat-copy-btn) {
    opacity: 1;
}

:deep(.chat-copy-btn:hover) {
    background: #f3f4f6;
    color: #24292f;
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
