import { ApolloClient } from '@apollo/client/core'
import { ApolloLink } from '@apollo/client/core'
import { createHttpLink } from '@apollo/client/core'
import { onError } from '@apollo/client/link/error'
import { InMemoryCache } from '@apollo/client/cache'
import fetch from 'isomorphic-unfetch'

const link = createHttpLink({
    fetch,
    uri: '/graphql',
})

const logoutLink = onError(({ graphQLErrors, networkError }) => {

    //not enough, sometimes errors are returned as graphQLErrors
    if (networkError && networkError.statusCode === 401)
        location.reload()

    if (graphQLErrors)
        graphQLErrors.forEach(({ extensions }) => {
            if (extensions && extensions.code && extensions.code === 'AUTH_NOT_AUTHENTICATED')
                location.reload()
        })
})

const cache = new InMemoryCache()

export default new ApolloClient({
    link: ApolloLink.from([logoutLink, link]),
    cache,
})