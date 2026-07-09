import { GraphQLClient, gql } from 'graphql-request'
import { validateFetchResponse } from '~/shared/serverValidator'

export { gql }

const client = new GraphQLClient(
    new URL('/graphql', window.location.origin).toString(),
    {
        fetch: async (...args) => {
            const response = await fetch(...args)
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