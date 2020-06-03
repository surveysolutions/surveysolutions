import { ApolloClient } from 'apollo-client'
import { createHttpLink } from 'apollo-link-http'
import { InMemoryCache } from 'apollo-cache-inmemory'
import fetch from 'isomorphic-unfetch'

const link = createHttpLink({
    fetch,
    uri: '/graphql',
})
const cache = new InMemoryCache()

export default new ApolloClient({
    link,
    cache,
})