import { ApolloClient } from 'apollo-client'
import { createHttpLink } from 'apollo-link-http'
import { InMemoryCache } from 'apollo-cache-inmemory'

const link = createHttpLink({ uri: '/graphql' })
const cache = new InMemoryCache()

export default new ApolloClient({
    link,
    cache,
})