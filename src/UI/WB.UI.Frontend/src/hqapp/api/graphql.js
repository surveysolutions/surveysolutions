import { GraphQLClient, gql } from 'graphql-request'
import { validateFetchResponse } from '~/shared/serverValidator'

export { gql }

const client = new GraphQLClient(
    // Fixed, same-origin relative endpoint. Keeping this a constant (never derived from
    // window.location or any request-controlled value) means the request destination is not
    // attacker-controllable, which removes the SSRF class entirely (CWE-918).
    '/graphql',
    {
        fetch: async (input, init = {}) => {
            // input is always our constant '/graphql'. mode/credentials 'same-origin' keep the
            // request (and any redirect) on the current origin as defense-in-depth.
            const response = await fetch(input, {
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