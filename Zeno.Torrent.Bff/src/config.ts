import dotenv from 'dotenv';

export interface Config {
    PORT: number
    BFF_ROOT: string
    AUTH_ROOT: string
    REALM: string
    CLIENT_ID: string
    CLIENT_SECRET: string
    CODE_CHALLENGE_TYPE: string
    RESPONSE_TYPE: string
    SCOPE: string
    SESSION_SECRET: string
    SESSION_NAME: string
    WEB_ORIGIN: string
    REDIS_PORT: number
    REDIS_HOST: string
    REDIS_PASSWORD: string
    SESSION_TIMEOUT: number
    NODE_ENV: 'dev' | 'prod' | 'test'
    API_ROOT: string
    API_ROUTE: string
    API_CLIENT_ID: string
    API_CLIENT_SECRET: string
    API_AUTH_ROOT: string
    API_REALM: string
}

const parseNodeEnv = (str: string): 'dev' | 'prod' | 'test' => {
    if (str !== 'prod' &&
        str !== 'test') {
        return 'dev';
    }
    return str;
}

dotenv.config();

const config: Config = {
    PORT: parseInt(process.env.PORT),
    BFF_ROOT: process.env.BFF_ROOT,
    AUTH_ROOT: process.env.AUTH_ROOT,
    REALM: process.env.REALM,
    CLIENT_ID: process.env.CLIENT_ID,
    CLIENT_SECRET: process.env.CLIENT_SECRET,
    CODE_CHALLENGE_TYPE: 'S256',
    RESPONSE_TYPE: 'code',
    SCOPE: 'openid profile email offline_access zeno',
    SESSION_SECRET: process.env.SESSION_SECRET,
    SESSION_NAME: process.env.SESSION_NAME,
    WEB_ORIGIN: process.env.WEB_ORIGIN,
    REDIS_PORT: parseInt(process.env.REDIS_PORT),
    REDIS_HOST: process.env.REDIS_HOST,
    REDIS_PASSWORD: process.env.REDIS_PASSWORD,
    NODE_ENV: parseNodeEnv(process.env.NODE_ENV),
    SESSION_TIMEOUT: 1000 * 60 * 60 * 24,
    API_ROOT: process.env.API_ROOT,
    API_ROUTE: process.env.API_ROUTE,
    API_CLIENT_ID: process.env.API_CLIENT_ID,
    API_CLIENT_SECRET: process.env.API_CLIENT_SECRET,
    API_AUTH_ROOT: process.env.API_AUTH_ROOT,
    API_REALM: process.env.API_REALM
};

export default config;