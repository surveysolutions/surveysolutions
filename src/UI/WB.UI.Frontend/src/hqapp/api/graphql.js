import { GraphQLClient, gql } from 'graphql-request'
import { validateFetchResponse } from '~/shared/serverValidator'

export { gql }

// graphql-request v7 requires a valid absolute URL for its internal URL parsing.
// This constant is never used for the actual request: the custom fetch below always
// targets the fixed same-origin relative path '/graphql', avoiding any dynamic input.
const GRAPHQL_ENDPOINT = '/graphql'

const client = new GraphQLClient(
    'https://placeholder.invalid/graphql',
    {
        fetch: async (input, init = {}) => {
            const response = await fetch(GRAPHQL_ENDPOINT, {
                ...init,
                mode: 'same-origin',
                credentials: 'same-origin',
            })
            validateFetchResponse(response)
            return response
        },
    }
)

export async function gqlRequest(document, variables) {
    try {
        return await client.request(document, variables)
    } catch (error) {
        if (error.response?.status === 401) {
            location.reload()
            return
        }
        const errors = error.response?.errors ?? []
        if (errors.some(e => e.extensions?.code === 'AUTH_NOT_AUTHENTICATED')) {
            location.reload()
            return
        }
        throw error
    }
}