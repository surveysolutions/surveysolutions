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

    const body = await response.text()
    let result = null
    if (body) {
        try {
            result = JSON.parse(body)
        } catch {
            result = null
        }
    }

    const errors = result?.errors ?? []

    if (errors.some(e => e.extensions?.code === 'AUTH_NOT_AUTHENTICATED')) {
        location.reload()
        return
    }

    // Throw unless this is a successful GraphQL envelope ({ data, ... }) without errors.
    // Guards against non-GraphQL responses on /graphql (e.g. 403 { Message: ... } from
    // ResetPasswordMiddleware), non-JSON bodies, and GraphQL error responses — all of
    // which would otherwise cause gqlRequest to silently return undefined.
    if (!response.ok || errors.length > 0 || result === null || result?.data === undefined) {
        const message =
            errors.map(e => e.message).join('\n') ||
            result?.Message ||
            result?.message ||
            body ||
            response.statusText ||
            `GraphQL request failed with status ${response.status}`
        throw new GraphQLRequestError(message, {
            status: response.status,
            errors,
            data: result?.data,
            body,
        })
    }

    return result.data
}