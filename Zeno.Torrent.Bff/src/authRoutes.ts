import express from 'express';
import crypto from 'crypto';
import config from './config';
import axios from 'axios';
import { isTokenValid } from './util/TokenUtil';

export interface OpenidConfig {
    issuer: string
    authorization_endpoint: string
    token_endpoint: string
    introspection_endpoint: string
    userinfo_endpoint: string
    end_session_endpoint : string
    jwks_uri: string
    check_session_iframe: string
    grant_types_supported: string[]
    response_types_supported: string[]
    subject_types_supported: string[]
    id_token_signing_alg_values_supported: string[]
    id_token_encryption_alg_values_supported: string[]
    id_token_encryption_enc_values_supported: string[]
    userinfo_signing_alg_values_supported: string[]
    request_object_signing_alg_values_supported: string[]
    response_modes_supported: string[]
    registration_endpoint: string
    token_endpoint_auth_methods_supported: string[]
    token_endpoint_auth_signing_alg_values_supported: string[]
    claims_supported: string[]
    claim_types_supported: string[]
    claims_parameter_supported: boolean
    scopes_supported: string[]
    request_parameter_supported: boolean
    request_uri_parameter_supported: boolean
    code_challenge_methods_supported: string[]
    tls_client_certificate_bound_access_tokens: boolean
}

function base64URLEncode(str: Buffer) {
    return str.toString('base64')
        .replace(/\+/g, '-')
        .replace(/\//g, '_')
        .replace(/=/g, '');
}
function sha256(buffer: Buffer) {
    return crypto.createHash('sha256').update(buffer).digest();
}

const createAuthRoutes = (authRoutePrefix: string, openidConnectConfig: OpenidConfig): express.Router => {
    const router = express.Router();

    router.get('/login', async (req, res) => {
        const verifier = base64URLEncode(crypto.randomBytes(32));
        const challenge = base64URLEncode(sha256(Buffer.from(verifier)));
        const state = base64URLEncode(crypto.randomBytes(32));
        const url = `${openidConnectConfig.authorization_endpoint}?` +
            `client_id=${config.CLIENT_ID}` +
            `&redirect_uri=${config.BFF_ROOT}/${authRoutePrefix}/logincallback` +
            `&response_type=${config.RESPONSE_TYPE}` +
            `&scope=${config.SCOPE}` +
            `&audience=${config.AUTH_ROOT}/realms/${config.REALM}` + 
            `&code_challenge_method=${config.CODE_CHALLENGE_TYPE}` +
            `&code_challenge=${challenge}` +
            `&state=${state}`;

        req.session.verifier = verifier;
        req.session.challenge = challenge;
        req.session.state = state;
        req.session.redirect_uri = req.query.redirect_uri as string;
        req.session.access_token = undefined;
        req.session.refresh_token = undefined;
        req.session.user = undefined;
    
        req.session.save();
        
        res.redirect(url);
    });

    router.get('/logincallback', async (req, res) => {
        const {
            state,
            code
        } = req.query;
        
        const {
            verifier
        } = req.session;
    
        if (state !== req.session.state) {
            return res.sendStatus(401);
        }
    
        const headers =  {'Content-type': 'application/x-www-form-urlencoded'};
    
        try {
            const response = await axios.post(openidConnectConfig.token_endpoint, 
                `grant_type=${'authorization_code'}&` +
                `code_verifier=${verifier}&` +
                `code=${code}&` +
                `client_id=${config.CLIENT_ID}&` +
                `client_secret=${config.CLIENT_SECRET}&` +
                `redirect_uri=${config.BFF_ROOT}/${authRoutePrefix}/logincallback`, {
                headers: headers
            });
            
            const userResponse = await axios.get(openidConnectConfig.userinfo_endpoint, {
                headers:  {
                    'Authorization': `bearer ${response.data.access_token}`
                }
            });
            
            let redirectUri = config.WEB_ORIGIN;
        
            if (!!req.session.redirect_uri) {
                redirectUri += req.session.redirect_uri;
            }
        
            req.session.verifier = undefined;
            req.session.challenge = undefined;
            req.session.state = undefined;
            req.session.redirect_uri = undefined;
            req.session.access_token = response.data.access_token;
            req.session.refresh_token = response.data.refresh_token;
            req.session.user = userResponse.data;
            console.log('user set', req.session.user);
        
            req.session.save();
        
            console.log(`'${req.session.user.name}' (${req.session.user.email}) has logged in (sub: ${req.session.user.sub})`);
        
            res.redirect(redirectUri);
        }
        catch (error) {            
            req.session.destroy(e => {});
            return res.redirect(`${config.WEB_ORIGIN}?status=${error}`);
        }
    });

    router.get('/logout', async (req, res) => {
        if (!req.session.refresh_token) {
            return res.redirect(config.WEB_ORIGIN);
        }
    
        if (!isTokenValid(openidConnectConfig, req.session.access_token, config.CLIENT_ID, config.CLIENT_SECRET)) {
            res.redirect('INVALID TOKEN');
        }
    
        const logout = `${openidConnectConfig.end_session_endpoint}?client_id=${config.CLIENT_ID}&refresh_token=${req.session.refresh_token}&redirect_uri=${config.BFF_ROOT}/${authRoutePrefix}/logoutcallback`;
        req.session.destroy((e) => {
            if (e) {
                res.redirect('FUCKEDUP')
            } else {
                res.redirect(logout);
            }
        });   
    });

    router.get('/logoutcallback', async (req, res) => {
        res.setHeader('Access-Control-Allow-Credentials', 'true');
        res.redirect(`${config.WEB_ORIGIN}`);
    });

    router.get('/user', (req, res) => {
        if (!isTokenValid(openidConnectConfig, req.session.access_token, config.CLIENT_ID, config.CLIENT_SECRET)) {
            return res.sendStatus(403);
        }

        res.setHeader('Access-Control-Allow-Credentials', 'true');
        res.status(200).send(req.session.user);
    });

    return router;
};

export default createAuthRoutes;