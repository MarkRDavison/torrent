import axios from "axios";
import { OpenidConfig } from "../authRoutes";
import config from "../config";

export interface IntrospectedToken {
    active: boolean
    exp: number
    iat: number
}

const introspectToken = async (openidConnectConfig: OpenidConfig, token: string, client_id: string, client_secret: string): Promise<IntrospectedToken> => {
    try {

        const headers =  {'Content-type': 'application/x-www-form-urlencoded'};
        const introspectResponse = await axios.post(openidConnectConfig.introspection_endpoint, 
            `client_id=${client_id}&` + 
            `client_secret=${client_secret}&` +
            `token=${token}`, {
                headers: headers
            });
        
        return introspectResponse.data;
    }
    catch {
        return {
            exp: (new Date().getTime()) / 1000 - 30,
            iat: (new Date().getTime()) / 1000 - 30,
            active: false
        };
    }
};

const isTokenValid = async (openidConnectConfig: OpenidConfig, token: string, client_id: string, client_secret: string): Promise<boolean> => {
    try {
        const {
            active
        } = await introspectToken(openidConnectConfig, token, client_id, client_secret);
        return active;
    }
    catch {
        return false;
    }
};

const isTokenRefreshNeeded = async (openidConnectConfig: OpenidConfig, token: string, margin: number, client_id: string, client_secret: string): Promise<boolean> => {
    const {
        exp,
        active
    } = await introspectToken(openidConnectConfig, token, client_id, client_secret);
    const now = (new Date().getTime()) / 1000;

    const need_refresh = 
        !active ||
        now + margin > exp;

    return need_refresh;
};

export {
    introspectToken,
    isTokenValid,
    isTokenRefreshNeeded
}