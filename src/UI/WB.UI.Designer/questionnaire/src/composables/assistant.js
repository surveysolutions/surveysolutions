import axios from 'axios';

export const useAssistant = () => {
    // Rate limiting: Track requests to avoid hitting limits
    let lastRequestTime = 0;
    const MIN_REQUEST_INTERVAL = 1000; // 1 second between requests

    const delay = (ms) => new Promise((resolve) => setTimeout(resolve, ms));

    const sendMessage = async (prompt, messages, options = {}) => {
        const retries = 3;

        // Rate limiting: Ensure minimum interval between requests
        const now = Date.now();
        const timeSinceLastRequest = now - lastRequestTime;
        if (timeSinceLastRequest < MIN_REQUEST_INTERVAL) {
            await delay(MIN_REQUEST_INTERVAL - timeSinceLastRequest);
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
                    },
                    {
                        timeout: 3 * 60 * 1000, // 3 minute timeout
                    },
                );

                return response.data.expression || response.data.message;
            } catch (error) {
                console.error(
                    `Assistant Error (attempt ${attempt}/${retries}):`,
                    error.response?.data || error.message,
                );

                if (error.response?.status === 401) {
                    throw new Error('Not authorized.');
                } else if (error.response?.status === 429) {
                    // Rate limit exceeded - implement exponential backoff
                    if (attempt < retries) {
                        const backoffDelay = Math.min(
                            1000 * Math.pow(2, attempt - 1),
                            10000,
                        ); // Max 10 seconds
                        console.log(
                            `Rate limit exceeded. Retrying in ${backoffDelay}ms...`,
                        );
                        await delay(backoffDelay);
                        continue;
                    } else {
                        throw new Error(
                            'Rate limit exceeded. Please check your OpenAI plan limits and try again later.',
                        );
                    }
                } else if (error.response?.status === 400) {
                    throw new Error(
                        'Invalid request. Please check your message format.',
                    );
                } else if (error.response?.status === 403) {
                    throw new Error(
                        'Access denied. Your API key may not have the required permissions.',
                    );
                } else if (error.response?.status === 404) {
                    throw new Error(
                        'Assistant model not found. Please check if you have access to the configured model.',
                    );
                } else if (error.response?.status >= 500) {
                    // Server error - retry
                    if (attempt < retries) {
                        const backoffDelay = Math.min(2000 * attempt, 10000);
                        console.log(
                            `Server error. Retrying in ${backoffDelay}ms...`,
                        );
                        await delay(backoffDelay);
                        continue;
                    } else {
                        throw new Error(
                            'Assistant service is temporarily unavailable. Please try again later.',
                        );
                    }
                } else {
                    throw new Error(
                        `Failed to connect to Assistant: ${error.message}`,
                    );
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
