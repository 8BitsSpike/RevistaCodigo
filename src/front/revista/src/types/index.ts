export interface AuthResponseSuccess {
    jwtToken: string;
    id: string; // O ID (ObjectId) do MongoDB como string
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