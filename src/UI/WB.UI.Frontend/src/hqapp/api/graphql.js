import { GraphQLClient, gql } from 'graphql-request'
import { validateFetchResponse } from '~/shared/serverValidator'

export { gql }

const client = new GraphQLClient(
    new URL('/graphql', window.location.origin).toString(),
    {
        fetch: async (input, init = {}) => {
            // Pin the GraphQL request to the current origin so it cannot be diverted to an
            // attacker-controlled destination, even across redirects (SSRF hardening, CWE-918).
            // Note: we use mode: 'same-origin' rather than redirect: 'error' so that the common
            // same-origin 302 -> /Account/LogOn auth-expiration redirect is still followed and
            // gqlRequest can react to it, while cross-origin redirects are blocked.
            const target = new URL(
                typeof input === 'string' ? input : input.url,
                window.location.origin
            )
            if (target.origin !== window.location.origin) {
                throw new Error('Refusing to send GraphQL request to a cross-origin destination')
            }

            const response = await fetch(target.toString(), {
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