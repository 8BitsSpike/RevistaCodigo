import { ApolloClient, InMemoryCache, createHttpLink, ApolloLink } from "@apollo/client";
import { setContext } from "@apollo/client/link/context";

const httpLink = createHttpLink({
    uri: "https://localhost:51413/graphql", // Endereço do endpoint 
});


const authLink = setContext((_, { headers }) => {
    // Pega o token de autenticação do localStorage.
    const token = localStorage.getItem("userToken");

    // Retorna os headers para o contexto para que o httpLink possa usá-los
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