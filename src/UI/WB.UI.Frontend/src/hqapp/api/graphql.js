import { gql } from 'graphql-request'
import { validateFetchResponse } from '~/shared/serverValidator'

export { gql }

// Fixed same-origin endpoint. Using a relative path keeps the request free of any
// dynamic input (no SSRF surface) and avoids graphql-request's absolute-URL requirement.
const GRAPHQL_ENDPOINT = '/graphql'

class GraphQLRequestError extends Error {
    constructor(message, response) {
        super(message)
        this.name = 'GraphQLRequestError'
        this.response = response
    }
}

function documentToString(document) {
    if (typeof document === 'string') return document
    // Support DocumentNode / TypedDocumentNode in case a caller passes one.
    return document?.loc?.source?.body ?? String(document)
}

export async function gqlRequest(document, variables) {
    const response = await fetch(GRAPHQL_ENDPOINT, {
        method: 'POST',
        mode: 'same-origin',
        credentials: 'same-origin',
        headers: {
            'Content-Type': 'application/json',
            Accept: 'application/json',
        },
        body: JSON.stringify({
            query: documentToString(document),
            variables,
        }),
    })

    validateFetchResponse(response)

    if (response.status === 401) {
        location.reload()
        return
    }

    const result = await response.json()
    const errors = result.errors ?? []

    if (errors.length > 0) {
        if (errors.some(e => e.extensions?.code === 'AUTH_NOT_AUTHENTICATED')) {
            location.reload()
            return
        }
        throw new GraphQLRequestError(
            errors.map(e => e.message).join('\n'),
            { status: response.status, errors, data: result.data }
        )
    }

    return result.data
}