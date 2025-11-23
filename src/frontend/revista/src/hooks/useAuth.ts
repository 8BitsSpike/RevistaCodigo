import { useState, useEffect } from 'react';
import client from '@/lib/apolloClient';
import { VERIFICAR_STAFF } from '@/graphql/queries';
import { USER_API_BASE } from '@/lib/fetcher';

interface UserState {
    id: string;
    jwtToken: string;
    name: string;
    foto?: string;
    isStaff: boolean;
}

interface LoginInput {
    id: string;
    jwtToken: string;
}

interface AuthHook {
    user: UserState | null;
    loading: boolean;
    login: (data: LoginInput) => Promise<void>;
    logout: () => void;
    isAuthenticated: boolean;
    isStaff: boolean;
}

export default function useAuth(): AuthHook {
    const [user, setUser] = useState<UserState | null>(null);
    const [loading, setLoading] = useState<boolean>(true);

    const login = async (data: LoginInput) => {
        try {
            console.log("useAuth: Starting login process for ID:", data.id);

            if (!data.jwtToken || !data.id) {
                throw new Error("Token ou ID inválidos recebidos no login.");
            }

            // 1. Persist Token
            localStorage.setItem('userToken', data.jwtToken);
            localStorage.setItem('userId', data.id);
            console.log(data.jwtToken);
            // 2. Reset Apollo to use new token
            await client.resetStore().catch(e => {
                console.warn("Apollo resetStore failed, trying clearStore", e);
                return client.clearStore();
            });

            // 3. Fetch User Profile (REST)
            const res = await fetch(`${USER_API_BASE}/${data.id}?token=${data.jwtToken}`, {
                headers: { 'Authorization': `Bearer ${data.jwtToken}` },
            });

            if (!res.ok) {
                throw new Error(`Erro ao buscar perfil (${res.status}).`);
            }
            const userData = await res.json();

            // 4. Check Staff Status (GraphQL)
            let isStaff = false;
            try {
                const { data: staffData } = await client.query({
                    query: VERIFICAR_STAFF,
                    fetchPolicy: 'network-only',
                });
                isStaff = staffData?.verificarStaff || false;
            } catch (gqlError) {
                console.warn("Erro ao verificar staff (pode não ser staff):", gqlError);
                isStaff = false;
            }

            // 5. Construct User Object
            const fullUser: UserState = {
                id: data.id,
                jwtToken: data.jwtToken,
                name: userData.name || '',
                foto: userData.url || userData.foto || '',
                isStaff: isStaff
            };

            // 6. Save remaining data
            localStorage.setItem('userName', fullUser.name);
            localStorage.setItem('userFoto', fullUser.foto || '');
            localStorage.setItem('isStaff', String(fullUser.isStaff));

            setUser(fullUser);
            console.log("useAuth: Login successful for", fullUser.name);

        } catch (error) {
            console.error("useAuth: Login failed:", error);
            localStorage.clear();
            setUser(null);
            throw error;
        }
    };

    const logout = async () => {
        localStorage.clear();
        setUser(null);
        try {
            await client.clearStore();
        } catch (e) {
            console.error("Error clearing apollo store", e);
        }
    };

    useEffect(() => {
        const token = localStorage.getItem('userToken');
        const userId = localStorage.getItem('userId');
        const userName = localStorage.getItem('userName');
        const userFoto = localStorage.getItem('userFoto');
        const isStaff = localStorage.getItem('isStaff') === 'true';

        if (token && userId) {
            setUser({
                id: userId,
                jwtToken: token,
                name: userName || '',
                foto: userFoto || undefined,
                isStaff: isStaff
            });
        }
        setLoading(false);
    }, []);

    return {
        user,
        loading,
        login,
        logout,
        isAuthenticated: !!user,
        isStaff: user?.isStaff || false
    };
}