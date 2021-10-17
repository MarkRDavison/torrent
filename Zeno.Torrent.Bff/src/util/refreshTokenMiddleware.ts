import axios from 'axios';
import express from 'express';
import { OpenidConfig } from '../authRoutes';
import config from '../config';
import { isTokenRefreshNeeded } from './TokenUtil';

export interface AuthTokens {
    access_token: string
    refresh_token: string
}

export const refreshToken = async (openidConnectConfig: OpenidConfig, tokens: AuthTokens, margin: number, client_id: string, client_secret: string): Promise<AuthTokens> => {
    const headers =  {
        'Content-type': 'application/x-www-form-urlencoded',            
        'Authorization': `bearer ${tokens.access_token}`
    };
    const response = await axios.post(openidConnectConfig.token_endpoint,
        `client_id=${client_id}&` +
        `grant_type=${'refresh_token'}&` +
        `client_secret=${client_secret}&` + 
        `refresh_token=${tokens.refresh_token}`, {
        headers: headers,
        withCredentials: true
    });
    
    return {
        access_token: response.data.access_token,
        refresh_token: response.data.refresh_token
    }
};

const CreateRefreshToken = (openidConnectConfig: OpenidConfig) => async (req: express.Request, res: express.Response, next: express.NextFunction): Promise<void> => {
    const {
        access_token,
        refresh_token
    } = req.session;

    try {
        if (!!access_token) {
            const need_refresh = await isTokenRefreshNeeded(openidConnectConfig, access_token, 30, config.CLIENT_ID, config.CLIENT_SECRET);
            
            if (need_refresh) {
                try {
                    const tokens = await refreshToken(openidConnectConfig, { access_token, refresh_token }, 30, config.CLIENT_ID, config.CLIENT_SECRET);

                    req.session.access_token = tokens.access_token;
                    req.session.refresh_token = tokens.refresh_token;

                    req.session.save();
                }
                catch (e) {
                    console.error(e);
                }
            }
        }
    }
    catch (err) {
        console.error(err);
    }

    next();
}

export default CreateRefreshToken;