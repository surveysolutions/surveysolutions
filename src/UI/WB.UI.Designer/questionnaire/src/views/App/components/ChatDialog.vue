<template>
    <teleport to="body">
        <div v-if="isOpen" class="modal chat-dialog dragAndDrop fade ng-scope ng-isolate-scope in" role="dialog"
            style="z-index: 1050; display: block;" v-dragAndDrop>
            <div class="modal-dialog" style="max-width: 800px;">
                <div class="modal-content">
                    <v-card class="chat-container" height="700" style="margin: 0;">
                        <v-card-title class="d-flex justify-space-between align-center pa-4 handle"
                            style="cursor: move;">
                            <div class="d-flex align-center">
                                <v-icon class="mr-2" color="primary">mdi-chat</v-icon>
                                <span>AI Assistant</span>
                            </div>
                            <v-btn icon="mdi-close" variant="text" size="small" @click="close" />
                        </v-card-title>

                        <v-divider />

                        <!-- Chat Messages -->
                        <v-card-text class="chat-messages pa-0" ref="messagesContainer"
                            style="height: 500px; overflow-y: auto;">
                            <div class="pa-4">
                                <div v-if="messages.length === 0" class="text-center text-grey-darken-1 mt-8">
                                    <v-icon size="48" class="mb-4">mdi-chat-outline</v-icon>
                                    <p>{{ $t('Chat.WelcomeMessage', 'Start a conversation with the AI assistant') }}</p>
                                </div>

                                <div v-for="(message, index) in messages" :key="message.id" class="mb-4">
                                    <div :class="[
                                        'message-bubble',
                                        message.role === 'user' ? 'user-message' : 'assistant-message'
                                    ]">
                                        <div class="d-flex align-start">
                                            <v-avatar :color="message.role === 'user' ? 'primary' : 'grey-lighten-2'"
                                                size="32" class="mr-3">
                                                <v-icon :color="message.role === 'user' ? 'white' : 'grey-darken-2'">
                                                    {{ message.role === 'user' ? 'mdi-account' : 'mdi-robot' }}
                                                </v-icon>
                                            </v-avatar>
                                            <div class="flex-grow-1">
                                                <div class="message-content">
                                                    <p class="mb-1" v-html="formatMessage(message.content)"></p>
                                                    <div class="d-flex align-center justify-space-between">
                                                        <small class="text-grey-darken-1">
                                                            {{ formatTime(message.timestamp) }}
                                                        </small>

                                                        <div v-if="message.role === 'assistant' && !message.isError"
                                                            class="d-flex align-center">
                                                            <v-btn variant="text" size="x-small"
                                                                :icon="getMessageReaction(message) === 1 ? 'mdi-thumb-up' : 'mdi-thumb-up-outline'"
                                                                :color="getMessageReaction(message) === 1 ? 'primary' : undefined"
                                                                @click="setReaction(message, index, 1)"
                                                                :title="getMessageReaction(message) === 1 ? $t('Chat.Unlike', 'Unlike') : $t('Chat.Like', 'Like')" />
                                                            <v-btn variant="text" size="x-small"
                                                                :icon="getMessageReaction(message) === -1 ? 'mdi-thumb-down' : 'mdi-thumb-down-outline'"
                                                                :color="getMessageReaction(message) === -1 ? 'error' : undefined"
                                                                @click="setReaction(message, index, -1)"
                                                                :title="getMessageReaction(message) === -1 ? $t('Chat.Undislike', 'Remove dislike') : $t('Chat.Dislike', 'Dislike')" />
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
                                            <v-avatar color="grey-lighten-2" size="32" class="mr-3">
                                                <v-icon color="grey-darken-2">mdi-robot</v-icon>
                                            </v-avatar>
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

                        <v-divider />

                        <!-- Input Area -->
                        <v-card-actions class="pa-4">
                            <v-text-field v-model="currentMessage"
                                :placeholder="$t('Chat.TypeMessage', 'Type your message...')" variant="outlined"
                                density="comfortable" hide-details @keyup.enter="sendMessage" :disabled="isLoading"
                                class="flex-grow-1">
                                <template v-slot:append-inner>
                                    <v-btn icon="mdi-send" variant="text" color="primary" size="small"
                                        @click="sendMessage" :disabled="!currentMessage.trim() || isLoading" />
                                </template>
                            </v-text-field>
                        </v-card-actions>
                    </v-card>
                </div>
            </div>
        </div>
        <div v-if="isOpen" class="modal-backdrop fade ng-scope in" style="z-index: 1040;" @click="close"></div>
    </teleport>
</template>

<script>
import { ref, nextTick, watch } from 'vue'
import { useAssistant } from '../../../composables/assistant'

export default {
    name: 'ChatDialog',
    emits: ['update:modelValue'],
    props: {
        modelValue: {
            type: Boolean,
            default: false
        },

        questionnaireId: { type: String, required: true },
        entityId: { type: String, required: false },
        area: { type: String, required: false }
    },
    setup(props, { emit }) {
        const isOpen = ref(props.modelValue)
        const messages = ref([])
        const currentMessage = ref('')
        const isLoading = ref(false)
        const messagesContainer = ref(null)

        // Initialize Assistant
        const { sendMessage: sendToAssistant, sendReaction: sendAssistantReaction } = useAssistant()

        // Watch for prop changes
        watch(() => props.modelValue, (newVal) => {
            isOpen.value = newVal
        })

        // Watch for dialog changes
        watch(isOpen, (newVal) => {
            emit('update:modelValue', newVal)
            // Reset chat history when dialog is opened
            if (newVal) {
                messages.value = []
                currentMessage.value = ''
            }
        })

        const open = () => {
            isOpen.value = true
        }

        const close = () => {
            isOpen.value = false
        }

        const scrollToBottom = async () => {
            await nextTick()
            if (messagesContainer.value) {
                messagesContainer.value.scrollTop = messagesContainer.value.scrollHeight
            }
        }

        const formatMessage = (content) => {
            // Simple formatting for line breaks and basic markdown
            return content
                .replace(/\n/g, '<br>')
                .replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>')
                .replace(/\*(.*?)\*/g, '<em>$1</em>')
        }

        const formatTime = (timestamp) => {
            return new Date(timestamp).toLocaleTimeString([], {
                hour: '2-digit',
                minute: '2-digit'
            })
        }

        const sendMessage = async () => {
            if (!currentMessage.value.trim()) return

            // Add user message
            const userMessage = {
                id: Date.now(),
                role: 'user',
                content: currentMessage.value,
                timestamp: Date.now()
            }

            messages.value.push(userMessage)

            const messageText = currentMessage.value
            currentMessage.value = ''
            isLoading.value = true

            await scrollToBottom()

            try {
                // Call Assistant with conversation history
                const response = await callAssistant(messageText, props.questionnaireId, props.entityId, props.area)

                const assistantMessage = {
                    id: Date.now() + 1,
                    role: 'assistant',
                    content: response,
                    timestamp: Date.now(),
                    reaction: 0
                }

                messages.value.push(assistantMessage)
                await scrollToBottom()
            } catch (error) {
                console.error('Error sending message:', error)

                const errorMessage = {
                    id: Date.now() + 1,
                    role: 'assistant',
                    content: error.message || 'Sorry, I encountered an error. Please try again.',
                    timestamp: Date.now(),
                    isError: true,
                    reaction: 0
                }

                messages.value.push(errorMessage)
                await scrollToBottom()
            } finally {
                isLoading.value = false
            }
        }

        const callAssistant = async (userMessage, questionnaireId, entityId, area) => {

            const conversationHistory = []

            // Add previous messages from the current conversation (exclude the current message)
            messages.value.forEach(msg => {
                conversationHistory.push({
                    role: msg.role,
                    content: msg.content
                })
            })

            // Call Assistant API with userMessage as a separate parameter
            return await sendToAssistant(userMessage, conversationHistory, {
                questionnaireId: questionnaireId,
                entityId: entityId,
                area: area
            })
        }

        const findPromptForAssistantMessage = (assistantIndex) => {
            for (let i = assistantIndex - 1; i >= 0; i--) {
                const msg = messages.value[i]
                if (msg && msg.role === 'user') return msg.content || ''
            }
            return ''
        }

        const getMessageReaction = (message) => {
            if (!message) return 0
            if (typeof message.reaction === 'number') return message.reaction
            if (message.isLiked === true) return 1
            if (message.isDisliked === true) return -1
            return 0
        }

        const setReaction = async (message, index, reactionValue) => {
            if (!message || message.role !== 'assistant' || message.isError) return

            const previous = getMessageReaction(message)
            const next = previous === reactionValue ? 0 : reactionValue

            message.reaction = next
            message.isLiked = next === 1
            message.isDisliked = next === -1

            try {
                await sendAssistantReaction(props.questionnaireId, {
                    entityId: props.entityId || null,
                    area: props.area || null,
                    clientMessageId: message.id,
                    clientTimestamp: message.timestamp,
                    prompt: findPromptForAssistantMessage(index),
                    assistantResponse: message.content,
                    reaction: next
                })
            } catch (error) {
                console.error('Error sending assistant reaction:', error)
                message.reaction = previous
                message.isLiked = previous === 1
                message.isDisliked = previous === -1
            }
        }

        return {
            isOpen,
            messages,
            currentMessage,
            isLoading,
            messagesContainer,
            open,
            close,
            sendMessage,
            getMessageReaction,
            setReaction,
            formatMessage,
            formatTime
        }
    }
}
</script>

<style scoped>
.chat-dialog {
    font-size: 14px;
}

.chat-container {
    display: flex;
    flex-direction: column;
    font-size: 14px;
}

.chat-container .v-card-title {
    display: flex !important;
    justify-content: space-between !important;
    font-size: 16px;
}

.chat-messages {
    flex: 1;
    overflow-y: auto;
    font-size: 14px;
}

.message-bubble {
    margin-bottom: 16px;
}

.message-bubble p {
    font-size: 14px;
}

.user-message .message-content {
    background-color: rgb(var(--v-theme-primary));
    color: white;
    padding: 12px 16px;
    border-radius: 18px;
    border-bottom-right-radius: 4px;
    font-size: 14px;
}

.assistant-message .message-content {
    background-color: rgb(var(--v-theme-surface-variant));
    color: rgb(var(--v-theme-on-surface-variant));
    padding: 12px 16px;
    border-radius: 18px;
    border-bottom-left-radius: 4px;
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

.chat-dialog {
    position: fixed;
}

.chat-dialog .modal-dialog {
    margin: 24px auto;
}

@media (max-width: 600px) {
    .chat-dialog .modal-dialog {
        margin: 12px auto;
    }

    .chat-container {
        height: calc(100vh - 24px) !important;
    }
}

.modal-backdrop {
    position: fixed;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    background-color: rgba(0, 0, 0, 0.5);
}
</style>
