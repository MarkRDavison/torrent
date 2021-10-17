import { Alert } from "../enums/Alerts";

export interface AlertState {
    type: Alert
    message?: string
}

const initialState: AlertState = {
    type: Alert.Clear,
    message: undefined
}

interface SuccessAlertAction {
    type: typeof Alert.Success
    payload: string
}

interface InfoAlertAction {
    type: typeof Alert.Info
    payload: string
}

interface ErrorAlertAction {
    type: typeof Alert.Error
    payload: string
}

interface WarningAlertAction {
    type: typeof Alert.Warning
    payload: string
}

interface ClearAlertAction {
    type: typeof Alert.Clear
    payload: undefined
}

export type AlertActionTypes =
    SuccessAlertAction |
    InfoAlertAction |
    ErrorAlertAction |
    WarningAlertAction |
    ClearAlertAction;

export function setSuccessAlert(message: string): AlertActionTypes {
    return {
        type: Alert.Success,
        payload: message
    };
};

export function setInfoAlert(message: string): AlertActionTypes {
    return {
        type: Alert.Info,
        payload: message
    };
};

export function setErrorAlert(message: string): AlertActionTypes {
    return {
        type: Alert.Error,
        payload: message
    };
};

export function setWarningAlert(message: string): AlertActionTypes {
    return {
        type: Alert.Warning,
        payload: message
    };
};

export function clearAlert(): AlertActionTypes {
    return {
        type: Alert.Clear,
        payload: undefined
    };
};

export function alertReducer(
    state = initialState,
    action: AlertActionTypes
): AlertState {
    if (action === undefined || action === null) {
        return state;
    }
    
    switch (action.type) {
        case Alert.Success:
        case Alert.Info:
        case Alert.Warning:
        case Alert.Error:
            return {
                ...state,
                message: action.payload,
                type: action.type
            };
        case Alert.Clear:
            return {
                ...state,
                message: undefined,
                type: Alert.Clear
            };
    }

    /* istanbul ignore next */
    return state;
}