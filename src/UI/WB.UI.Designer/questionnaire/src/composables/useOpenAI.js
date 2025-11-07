import axios from 'axios'

export const useOpenAI = () => {
  // Get API key from environment variables only - never hardcode it
  const apiKey = 'your-api-key-here' || process.env.VUE_APP_OPENAI_API_KEY
  
  if (!apiKey) {
    throw new Error('OpenAI API key is not configured. Please set VUE_APP_OPENAI_API_KEY in your environment variables.')
  }
  
  // Rate limiting: Track requests to avoid hitting limits
  let lastRequestTime = 0
  const MIN_REQUEST_INTERVAL = 1000 // 1 second between requests
  
  const delay = (ms) => new Promise(resolve => setTimeout(resolve, ms))
  
  const sendMessage = async (messages, options = {}) => {
    const {
      model = process.env.VUE_APP_OPENAI_MODEL || 'gpt-3.5-turbo',
      maxTokens = parseInt(process.env.VUE_APP_OPENAI_MAX_TOKENS) || 500,
      temperature = 0.7,
      retries = 3
    } = options
    
    // Rate limiting: Ensure minimum interval between requests
    const now = Date.now()
    const timeSinceLastRequest = now - lastRequestTime
    if (timeSinceLastRequest < MIN_REQUEST_INTERVAL) {
      await delay(MIN_REQUEST_INTERVAL - timeSinceLastRequest)
    }
    lastRequestTime = Date.now()
    
    for (let attempt = 1; attempt <= retries; attempt++) {
      try {
        const response = await axios.post(
          'https://api.openai.com/v1/chat/completions',
          {
            model,
            messages: messages.map(msg => ({
              role: msg.role,
              content: msg.content
            })),
            max_tokens: maxTokens,
            temperature
          },
          {
            headers: {
              'Authorization': `Bearer ${apiKey}`,
              'Content-Type': 'application/json'
            },
            timeout: 30000 // 30 second timeout
          }
        )
        
        return response.data.choices[0].message.content
      } catch (error) {
        console.error(`OpenAI API Error (attempt ${attempt}/${retries}):`, error.response?.data || error.message)
        
        if (error.response?.status === 401) {
          throw new Error('Invalid API key. Please check your OpenAI API key.')
        } else if (error.response?.status === 429) {
          // Rate limit exceeded - implement exponential backoff
          if (attempt < retries) {
            const backoffDelay = Math.min(1000 * Math.pow(2, attempt - 1), 10000) // Max 10 seconds
            console.log(`Rate limit exceeded. Retrying in ${backoffDelay}ms...`)
            await delay(backoffDelay)
            continue
          } else {
            throw new Error('Rate limit exceeded. Please check your OpenAI plan limits and try again later.')
          }
        } else if (error.response?.status === 400) {
          throw new Error('Invalid request. Please check your message format.')
        } else if (error.response?.status === 403) {
          throw new Error('Access denied. Your API key may not have the required permissions.')
        } else if (error.response?.status === 404) {
          throw new Error(`Model "${model}" not found. Please check if you have access to this model.`)
        } else if (error.response?.status >= 500) {
          // Server error - retry
          if (attempt < retries) {
            const backoffDelay = Math.min(2000 * attempt, 10000)
            console.log(`Server error. Retrying in ${backoffDelay}ms...`)
            await delay(backoffDelay)
            continue
          } else {
            throw new Error('OpenAI service is temporarily unavailable. Please try again later.')
          }
        } else {
          throw new Error(`Failed to connect to OpenAI: ${error.message}`)
        }
      }
    }
  }
  
  const createSystemMessage = (content) => ({
    role: 'system',
    content
  })
  
  const createUserMessage = (content) => ({
    role: 'user',
    content
  })
  
  const createAssistantMessage = (content) => ({
    role: 'assistant',
    content
  })
  
  return {
    sendMessage,
    createSystemMessage,
    createUserMessage,
    createAssistantMessage
  }
}

// Example usage in your ChatDialog component:
/*
import { useOpenAI } from '@/composables/useOpenAI'

// In your setup function:
const { sendMessage, createSystemMessage } = useOpenAI()

// When sending a message:
const callOpenAI = async (userMessage) => {
  const systemMessage = createSystemMessage(
    'You are a helpful assistant for Survey Solutions questionnaire design. ' +
    'Help users with questionnaire structure, question types, validation rules, and best practices.'
  )
  
  const conversationHistory = [systemMessage, ...messages.value]
  conversationHistory.push({
    role: 'user',
    content: userMessage
  })
  
  return await sendMessage(conversationHistory)
}
*/