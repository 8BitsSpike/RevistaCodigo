import { ApolloClient, InMemoryCache, createHttpLink } from "@apollo/client";
import { setContext } from "@apollo/client/link/context";

//    O 'httpLink' é a conexão direta com sua API
//    A URL vem do seu launchSettings.json
const httpLink = createHttpLink({
    uri: "https://localhost:51413/graphql", // Endpoint do Hot Chocolate
});

//    O 'authLink' é responsável por adicionar o token de autenticação
//    a CADA requisição para a ArtigoAPI.
const authLink = setContext((_, { headers }) => {
    // Pega o token de autenticação do localStorage.
    // Estamos usando 'userToken' (que definimos na página de login)
    const token = localStorage.getItem("userToken");

    return {
        headers: {
            ...headers,
            authorization: token ? `Bearer ${token}` : "",
        },
    };
});

//     O 'client' é a instância do Apollo que sua aplicação usará.
const client = new ApolloClient({
    // Ele combina o authLink e o httpLink.
    link: authLink.concat(httpLink),

    // O cache é usado pelo Apollo para armazenar resultados de query
    cache: new InMemoryCache(),
});

export default client;