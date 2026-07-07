import { GraphQLClient, gql } from 'graphql-request'
import { validateFetchResponse } from '~/shared/serverValidator'

export { gql }

const client = new GraphQLClient(
    new URL('/graphql', window.location.origin).toString(),
    {
        fetch: async (input, init = {}) => {
            // Pin the GraphQL request to the current origin and forbid redirects so it
            // can't follow a redirect to another origin (preventing cross-origin leakage).
            const target = new URL(
                typeof input === 'string' ? input : input.url,
                window.location.origin
            )
            if (target.origin !== window.location.origin) {
                throw new Error('Refusing to send GraphQL request to a cross-origin destination')
            }

            const response = await fetch(target.toString(), {
                ...init,
                redirect: 'error',
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