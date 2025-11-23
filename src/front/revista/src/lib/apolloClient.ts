import { ApolloClient, InMemoryCache, createHttpLink } from "@apollo/client";
import { setContext } from "@apollo/client/link/context";

const httpLink = createHttpLink({
    uri: "https://localhost:44363/graphql",
});

const authLink = setContext((_, { headers }) => {

    let token = '';
    if (typeof window !== 'undefined') {
        token = localStorage.getItem("userToken") || '';
    }

    return {
        headers: {
            ...headers,
            authorization: token ? `Bearer ${token}` : "",
        },
    };
});

const client = new ApolloClient({
    link: authLink.concat(httpLink),
    cache: new InMemoryCache(),
});

export default client;