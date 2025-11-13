import { useState, useEffect } from 'react';
import { AuthResponseSuccess } from '@/types';

interface UserState {
    id: string;
    name: string;
    foto: string | null;
}

interface AuthData {
    id: string;
    jwtToken: string;
    name: string;
    sobrenome: string;
    foto: string | null;
}

interface AuthHook {
    user: UserState | null;
    loading: boolean;
    login: (data: AuthData) => void;
    logout: () => void;
}

export default function useAuth(): AuthHook {

    const [user, setUser] = useState<UserState | null>(null);
    const [loading, setLoading] = useState<boolean>(true);

    const login = (data: AuthData) => {
        const fullName = `${data.name || ''} ${data.sobrenome || ''}`.trim();

        localStorage.setItem('userToken', data.jwtToken);
        localStorage.setItem('userId', data.id);
        localStorage.setItem('userName', fullName);
        if (data.foto) {
            localStorage.setItem('userFoto', data.foto);
        }

        setUser({ id: data.id, name: fullName, foto: data.foto || null });
    };

    const logout = () => {
        localStorage.removeItem('userToken');
        localStorage.removeItem('userId');
        localStorage.removeItem('userName');
        localStorage.removeItem('userFoto');
        localStorage.removeItem('isStaff');

        setUser(null);
    };

    useEffect(() => {
        const token = localStorage.getItem('userToken');
        const userId = localStorage.getItem('userId');
        const userName = localStorage.getItem('userName');
        const userFoto = localStorage.getItem('userFoto');

        if (token && userId && userName) {
            setUser({
                id: userId,
                name: userName,
                foto: userFoto || null
            });
        } else {
            logout();
        }

        setLoading(false);
    }, []);

    return { user, loading, login, logout };
}