<template>
    <v-dialog v-model="isOpen" max-width="800" persistent class="chat-dialog">
        <v-card class="chat-container" height="700">
            <v-card-title class="d-flex justify-space-between align-center pa-4">
                <div class="d-flex align-center">
                    <v-icon class="mr-2" color="primary">mdi-chat</v-icon>
                    <span>AI Assistant</span>
                </div>
                <v-btn icon="mdi-close" variant="text" size="small" @click="close" />
            </v-card-title>

            <v-divider />

            <!-- Chat Messages -->
            <v-card-text class="chat-messages pa-0" ref="messagesContainer" style="height: 500px; overflow-y: auto;">
                <div class="pa-4">
                    <div v-if="messages.length === 0" class="text-center text-grey-darken-1 mt-8">
                        <v-icon size="48" class="mb-4">mdi-chat-outline</v-icon>
                        <p>{{ $t('Chat.WelcomeMessage', 'Start a conversation with the AI assistant') }}</p>
                    </div>

                    <div v-for="message in messages" :key="message.id" class="mb-4">
                        <div :class="[
                            'message-bubble',
                            message.role === 'user' ? 'user-message' : 'assistant-message'
                        ]">
                            <div class="d-flex align-start">
                                <v-avatar :color="message.role === 'user' ? 'primary' : 'grey-lighten-2'" size="32"
                                    class="mr-3">
                                    <v-icon :color="message.role === 'user' ? 'white' : 'grey-darken-2'">
                                        {{ message.role === 'user' ? 'mdi-account' : 'mdi-robot' }}
                                    </v-icon>
                                </v-avatar>
                                <div class="flex-grow-1">
                                    <div class="message-content">
                                        <p class="mb-1" v-html="formatMessage(message.content)"></p>
                                        <small class="text-grey-darken-1">
                                            {{ formatTime(message.timestamp) }}
                                        </small>
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
                <v-text-field v-model="currentMessage" :placeholder="$t('Chat.TypeMessage', 'Type your message...')"
                    variant="outlined" density="comfortable" hide-details @keyup.enter="sendMessage"
                    :disabled="isLoading" class="flex-grow-1">
                    <template v-slot:append-inner>
                        <v-btn icon="mdi-send" variant="text" color="primary" size="small" @click="sendMessage"
                            :disabled="!currentMessage.trim() || isLoading" />
                    </template>
                </v-text-field>
            </v-card-actions>
        </v-card>
    </v-dialog>
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
        }
    },
    setup(props, { emit }) {
        const isOpen = ref(props.modelValue)
        const messages = ref([])
        const currentMessage = ref('')
        const isLoading = ref(false)
        const messagesContainer = ref(null)

        // Initialize OpenAI composable
        const { sendMessage: sendToOpenAI, createSystemMessage } = useAssistant()

        // Watch for prop changes
        watch(() => props.modelValue, (newVal) => {
            isOpen.value = newVal
        })

        // Watch for dialog changes
        watch(isOpen, (newVal) => {
            emit('update:modelValue', newVal)
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
                // Call OpenAI API with conversation history
                const response = await callOpenAI(messageText)

                const assistantMessage = {
                    id: Date.now() + 1,
                    role: 'assistant',
                    content: response,
                    timestamp: Date.now()
                }

                messages.value.push(assistantMessage)
                await scrollToBottom()
            } catch (error) {
                console.error('Error sending message:', error)

                const errorMessage = {
                    id: Date.now() + 1,
                    role: 'assistant',
                    content: error.message || 'Sorry, I encountered an error. Please try again.',
                    timestamp: Date.now()
                }

                messages.value.push(errorMessage)
                await scrollToBottom()
            } finally {
                isLoading.value = false
            }
        }

        // OpenAI API call with conversation context
        const callOpenAI = async (userMessage) => {
            // Create system message with Survey Solutions context
            const systemMessage = createSystemMessage(
                'You are a helpful AI assistant specialized in Survey Solutions questionnaire design. ' +
                'You help users with questionnaire structure, question types, validation rules, conditional logic, ' +
                'roster design, variable naming conventions, and best practices for survey creation. ' +
                'Provide clear, actionable advice and examples when possible. ' +
                'Keep responses concise but informative.'
            )

            // Build conversation history including system message
            const conversationHistory = [systemMessage]

            // Add previous messages from the current conversation
            messages.value.forEach(msg => {
                conversationHistory.push({
                    role: msg.role,
                    content: msg.content
                })
            })

            // Add the current user message
            conversationHistory.push({
                role: 'user',
                content: userMessage
            })

            // Call OpenAI API
            return await sendToOpenAI(conversationHistory, {
                model: 'gpt-3.5-turbo',
                maxTokens: 800,
                temperature: 0.7
            })
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
            formatMessage,
            formatTime
        }
    }
}
</script>

<style scoped>
.chat-container {
    display: flex;
    flex-direction: column;
}

.chat-messages {
    flex: 1;
    overflow-y: auto;
}

.message-bubble {
    margin-bottom: 16px;
}

.user-message .message-content {
    background-color: rgb(var(--v-theme-primary));
    color: white;
    padding: 12px 16px;
    border-radius: 18px;
    border-bottom-right-radius: 4px;
}

.assistant-message .message-content {
    background-color: rgb(var(--v-theme-surface-variant));
    color: rgb(var(--v-theme-on-surface-variant));
    padding: 12px 16px;
    border-radius: 18px;
    border-bottom-left-radius: 4px;
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

.chat-dialog .v-dialog {
    margin: 24px;
}

@media (max-width: 600px) {
    .chat-dialog .v-dialog {
        margin: 12px;
    }

    .chat-container {
        height: calc(100vh - 24px) !important;
    }
}
</style>