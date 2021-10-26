import express from 'express';
import cors from 'cors';
import config from './config';
import axios from 'axios';
import session from 'express-session';
import connectRedis from 'connect-redis';
import redis from 'redis';
import CreateRefreshToken, { AuthTokens, refreshToken } from './util/refreshTokenMiddleware';
import { createProxyMiddleware } from 'http-proxy-middleware';
import createAuthRoutes, { OpenidConfig } from './authRoutes';
import createHealthRoutes from './healthRoutes';
import { isTokenRefreshNeeded } from './util/TokenUtil';

interface UserProfile {
    sub: string
    email_verified: boolean
    name: string
    preferred_username: string
    given_name: string
    family_name: string
    email: string
}

declare module 'express-session' {
    interface SessionData {
          verifier: string
          challenge: string
          state: string
          redirect_uri: string
          access_token: string
          refresh_token: string
          user: UserProfile
    }
}

let authState: AuthTokens = {
    access_token: '',
    refresh_token: ''
};

const initOpenIdConfig = async (authRoot: string, realm: string): Promise<OpenidConfig> => {    
    let openidConnectConfig: OpenidConfig | undefined = undefined;

    let count = 0;
    while (openidConnectConfig === undefined && count < 20) {        
        await new Promise(resolve => setTimeout(resolve, 1000 * count));
        try {
            const url = `${authRoot}/realms/${realm}/.well-known/openid-configuration`;
            const response = await axios.get(url);
            openidConnectConfig = response.data as OpenidConfig;
        }
        catch (error) {
            console.error(error);
        }
        count++;
    }

    if (openidConnectConfig === undefined) {
        throw new Error("FAILED TO FETCH OPENID CONNECT CONFIG");
    }

    return openidConnectConfig;
};

const init = async (): Promise<express.Application> => {
    const RedisStore = connectRedis(session);
    const redisClient = redis.createClient({
        host: config.REDIS_HOST,
        port: config.REDIS_PORT,
        password: config.REDIS_PASSWORD || undefined
    });
    
    redisClient.on('error', (err) => {
        console.log('Could not establish a connection with redis', err);
    });
    
    const openidConnectConfigBff = await initOpenIdConfig(config.AUTH_ROOT, config.REALM);
    const openidConnectConfigApi = 
        (config.AUTH_ROOT === config.API_AUTH_ROOT && config.REALM === config.API_REALM)
            ? openidConnectConfigBff
            : await initOpenIdConfig(config.API_AUTH_ROOT, config.API_REALM);
    
    const app = express();
    app.use(cors({ origin: config.WEB_ORIGIN, credentials: true }));
    app.use(session({
        store: new RedisStore({ client: redisClient }),
        secret: config.SESSION_SECRET,
        resave: false,
        saveUninitialized: false,
        name: config.SESSION_NAME,
        cookie: {
            signed: true,
            secure: config.NODE_ENV === 'prod',
            httpOnly: true,
            maxAge: config.SESSION_TIMEOUT
        }
    }));
    app.use(CreateRefreshToken(openidConnectConfigBff));
    app.use(async (req, res, next) => {
        try {
            if (!!authState.access_token && !!authState.refresh_token) {
                // We have an access token, check if it is still valid
                if (await isTokenRefreshNeeded(openidConnectConfigApi, authState.access_token, 30, config.API_CLIENT_ID, config.API_CLIENT_SECRET)) {
                    const tokens = await refreshToken(openidConnectConfigApi, authState, 30, config.API_CLIENT_ID, config.API_CLIENT_SECRET);
                    authState.access_token = tokens.access_token;
                    authState.refresh_token = tokens.refresh_token;
                }
            } else {
                // We don't have an access token, we need to get one
                const headers =  {
                    'Content-type': 'application/x-www-form-urlencoded'
                };
                const response = await axios.post(openidConnectConfigApi.token_endpoint,
                    `scope=${'zeno'}&` +
                    `grant_type=${'client_credentials'}&` +
                    `client_id=${config.API_CLIENT_ID}&` +
                    `client_secret=${config.API_CLIENT_SECRET}`, {
                    headers: headers,
                    withCredentials: true
                });

                authState.access_token = response.data.access_token;
                authState.refresh_token = response.data.refresh_token;
            }
        }
        catch (e) {
            console.error('There was an error trying to update the auth state for the api');
            console.error(e);
            authState.access_token = undefined;
            authState.refresh_token = undefined;
        }
        next();
    });
    app.use(config.API_ROUTE, createProxyMiddleware({
        target: config.API_ROOT,
        changeOrigin: true,
        onProxyReq: (proxyReq, req: express.Request, res: express.Response) => {
            console.log(`BEGIN - BFF Proxying request: ${req.method} - ${req.path}`);
            if (!!authState.access_token) {
                proxyReq.setHeader('Authorization', `bearer ${authState.access_token}`);
            }
            if (!!req.session.user) {
                proxyReq.setHeader('sub', req.session.user.sub);
            }
        },
        onProxyRes: (proxyRes, req: express.Request, res: express.Response) => {
            if (!!proxyRes.headers['www-authenticate']){
                console.log('www-authenticate: ', proxyRes.headers['www-authenticate']);
            }
            console.log(`END - BFF Proxied request: ${req.method} - ${req.path} - ${proxyRes.statusCode}`);

            if (proxyRes.statusCode >= 400 && 499 <= proxyRes.statusCode) {                
                authState.access_token = undefined;
                authState.refresh_token = undefined;
            }
        }
    }));
    app.use(express.json()); // This must be after the proxy
    app.set('trust proxy', true);
    
    app.use('/health', createHealthRoutes());
    app.use('/auth', createAuthRoutes('auth', openidConnectConfigBff));
    
    return app;
};

init()
    .then(app => {
        app.listen(config.PORT, async () => {
            console.log(`Listening on Port ${config.PORT}`);
        })
    })
    .catch(e => {
        console.error(e)
    });