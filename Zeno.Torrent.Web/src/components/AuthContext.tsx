import axios from 'axios';
import React, { useContext, useEffect, useState } from 'react';
import config from '../util/config';

export interface UserProfile {
    sub: string
    email_verified: boolean
    name: string
    preferred_username: string
    given_name: string
    family_name: string
    email: string
}

interface ContextProps {
    user: UserProfile | null
    isLoggedIn: boolean
    isLoggingIn: boolean
    login: () => void
    logout: () => void   
}

const AuthContext = React.createContext<ContextProps>({
    user: null,
    isLoggedIn: false,
    isLoggingIn: true,
    login: () => {},
    logout: () => {}
});

interface AuthProviderProps {
    children?: JSX.Element
}
export const AuthConsumer = AuthContext.Consumer;
export const AuthProvider = (props: AuthProviderProps) => {
    const [user, setUser] = useState<UserProfile | null>(null);
    const [isLoggedIn, setIsLoggedIn] = useState(false);
    const [isLoggingIn, setIsLoggingIn] = useState(true);
    useEffect(() => {
        setIsLoggedIn(false);
        setIsLoggingIn(true)
        axios
            .get(`${config.bff_base_uri}/auth/user`, {
                withCredentials: true
            })
            .then(r => {
                if (r.data) {
                    setUser(r.data);
                    setIsLoggedIn(true);
                    setIsLoggingIn(false);
                }
                else {                    
                    setUser(null);
                    setIsLoggedIn(false);
                    setIsLoggingIn(false);
                }
            })
            .catch(e => {
                console.error(e);
                setUser(null);
                setIsLoggedIn(false);
                setIsLoggingIn(false);
            });
    }, [setUser, setIsLoggingIn, setIsLoggedIn]);
    
    return (
        <AuthContext.Provider value={{
            user: user,
            isLoggedIn: isLoggedIn,
            isLoggingIn: isLoggingIn,
            login: () => window.location.href = `${config.login_uri}?redirect_uri=${window.location.pathname}`,
            logout:() => window.location.href = config.logout_uri
        }}>
            {props.children}
        </AuthContext.Provider>
    );
}

export const useAuth = (): ContextProps => useContext(AuthContext);