import { useState, useEffect } from 'react';
import { AuthResponseSuccess } from '@/types';

interface UserState {
    id: string;
    jwtToken: string;
}

interface AuthData {
    id: string;
    jwtToken: string;
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
        localStorage.setItem('userToken', data.jwtToken);
        localStorage.setItem('userId', data.id);

        setUser({ id: data.id, jwtToken: data.jwtToken, });
    };

    const logout = () => {
        localStorage.removeItem('userToken');
        localStorage.removeItem('userId');
        localStorage.removeItem('userName');
        localStorage.removeItem('userSobrenome');
        localStorage.removeItem('userFullname');
        localStorage.removeItem('userFoto');
        localStorage.removeItem('isStaff');
        setUser(null);
    };

    useEffect(() => {
        const token = localStorage.getItem('userToken');
        const userId = localStorage.getItem('userId');
        if (token && userId) {
            setUser({
                id: userId,
                jwtToken: token,
            });
        } else {
            logout();
        }
        setLoading(false);
    }, []);
    return { user, loading, login, logout };
}