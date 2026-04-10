import axios from 'axios';
import { i18n } from '../plugins/localization';

export const useAssistant = () => {
    // Rate limiting: Track requests to avoid hitting limits
    let lastRequestTime = 0;
    const MIN_REQUEST_INTERVAL = 1000; // 1 second between requests

    const delay = (ms) => new Promise((resolve) => setTimeout(resolve, ms));

    const createAbortError = (signal) => {
        if (signal && 'reason' in signal && signal.reason != null) {
            if (signal.reason instanceof Error) {
                return signal.reason;
            }
            const reasonMessage =
                typeof signal.reason === 'string' ? signal.reason : 'Aborted';
            const error = new Error(reasonMessage);
            error.name = 'AbortError';
            return error;
        }
        const error = new Error('Aborted');
        error.name = 'AbortError';
        return error;
    };

    const abortAwareDelay = (ms, signal) => {
        if (!signal) return delay(ms);
        if (signal.aborted) return Promise.reject(createAbortError(signal));
        return new Promise((resolve, reject) => {
            let timer;
            const onAbort = () => {
                clearTimeout(timer);
                reject(createAbortError(signal));
            };
            signal.addEventListener('abort', onAbort, { once: true });
            timer = setTimeout(() => {
                signal.removeEventListener('abort', onAbort);
                resolve();
            }, ms);
        });
    };

    const sendMessage = async (prompt, messages, options = {}) => {
        const retries = 3;

        // Rate limiting: Ensure minimum interval between requests
        const now = Date.now();
        const timeSinceLastRequest = now - lastRequestTime;
        if (timeSinceLastRequest < MIN_REQUEST_INTERVAL) {
            await abortAwareDelay(
                MIN_REQUEST_INTERVAL - timeSinceLastRequest,
                options.signal,
            );
        }
        lastRequestTime = Date.now();

        for (let attempt = 1; attempt <= retries; attempt++) {
            try {
                const questionnaireId = options.questionnaireId || null;
                const response = await axios.post(
                    `/api/assistance/${questionnaireId}`,
                    {
                        messages: messages.map((msg) => ({
                            role: msg.role,
                            content: msg.content,
                        })),
                        prompt: prompt,
                        entityId: options.entityId || null,
                        conversationId: options.conversationId || null,
                    },
                    {
                        timeout: 3 * 60 * 1000, // 3 minute timeout
                        signal: options.signal || null,
                    },
                );

                const data = response.data || {};
                const text =
                    data.expression ?? data.answer ?? data.message ?? '';
                const meta = data.meta ?? data.Meta ?? null;
                const conversationId = data.conversationId ?? null;
                const callLogId = data.callLogId ?? null;

                return { text, meta, conversationId, callLogId };
            } catch (error) {
                // Re-throw abort errors immediately without retrying
                if (
                    error.name === 'AbortError' ||
                    error.name === 'CanceledError' ||
                    error.code === 'ERR_CANCELED'
                ) {
                    throw error;
                }

                if (error.response?.status === 401) {
                    throw new Error(i18n.t('Assistant.NotAuthorized'));
                } else if (error.response?.status === 429) {
                    // Rate limit exceeded - implement exponential backoff
                    if (attempt < retries) {
                        const backoffDelay = Math.min(
                            1000 * Math.pow(2, attempt - 1),
                            10000,
                        ); // Max 10 seconds
                        await abortAwareDelay(backoffDelay, options.signal);
                        continue;
                    } else {
                        throw new Error(i18n.t('Assistant.RateLimitExceeded'));
                    }
                } else if (error.response?.status === 400) {
                    throw new Error(i18n.t('Assistant.InvalidRequest'));
                } else if (error.response?.status === 403) {
                    throw new Error(i18n.t('Assistant.AccessDenied'));
                } else if (error.response?.status === 404) {
                    throw new Error(i18n.t('Assistant.ModelNotFound'));
                } else if (error.response?.status === 422) {
                    throw new Error(i18n.t('Assistant.ServiceUnavailable'));
                } else if (error.response?.status >= 500) {
                    // Server error - retry
                    if (attempt < retries) {
                        const backoffDelay = Math.min(2000 * attempt, 10000);
                        await abortAwareDelay(backoffDelay, options.signal);
                        continue;
                    } else {
                        throw new Error(i18n.t('Assistant.ServiceUnavailable'));
                    }
                } else {
                    const err = new Error(i18n.t('Assistant.ConnectionError'));
                    throw err;
                }
            }
        }
    };

    const sendReaction = async (questionnaireId, reaction) => {
        if (!questionnaireId) throw new Error('questionnaireId is required');

        await axios.post(
            `/api/assistance/${questionnaireId}/reaction`,
            reaction,
            {
                timeout: 30 * 1000,
            },
        );
    };

    const createUserMessage = (content) => ({
        role: 'user',
        content,
    });

    const createAssistantMessage = (content) => ({
        role: 'assistant',
        content,
    });

    return {
        sendMessage,

        sendReaction,

        createUserMessage,
        createAssistantMessage,
    };
};
