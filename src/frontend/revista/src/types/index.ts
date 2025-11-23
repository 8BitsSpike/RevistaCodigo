export interface AuthResponseSuccess {
    jwtToken: string;
    id: string;
    name: string;
    sobrenome: string;
}
export interface UserCredentials {
    email: string;
    password: string;
}

export type User = {
    id: string;
    name: string;
    email?: string;
    role?: 'admin' | 'reader';
};

export type Post = {
    id: string;
    title: string;
    slug: string;
    excerpt?: string;
    content?: string;
    publishedAt?: string;
    authorId?: string;
};

export type Edition = {
    id: string;
    slug: string;
    title: string;
    publishedAt?: string;
};

export type Review = {
    id: string;
    postId: string;
    reviewerId: string;
    verdict?: string;
};
export interface StaffComentario {
    id: string;
    usuarioId: string;
    data: string;
    parent: string | null;
    comment: string;
    __typename: "StaffComentario";
}