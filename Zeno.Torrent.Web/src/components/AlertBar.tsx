import React from "react";
import Alert from '@material-ui/lab/Alert';
import { connect } from "react-redux";
import { DispatchType, RootState } from '../store/Store';
import { Color } from "@material-ui/lab/Alert";
import * as AlertEnum from '../enums/Alerts';
import { clearAlert } from "../store/AlertReducer";

interface StateProps {
    type: AlertEnum.Alert
    message: string | undefined
}
interface DispatchProps {
    onClose: () => void
}
type Props = StateProps & DispatchProps;

const _AlertBar = (props: Props): JSX.Element => {
    if (props.type === AlertEnum.Alert.Clear){
        return <div/>
    }

    return (
        <Alert
            severity={props.type as Color}
            onClose={props.onClose}
        >{props.message}</Alert>
    );
};

const mapStateToProps = (state: RootState): StateProps => {
    return {
        type: state.alertState.type,
        message: state.alertState.message
    };
};

const mapDispatchToProps = (dispatch: DispatchType): DispatchProps => {
    return {
        onClose: () => dispatch(clearAlert())
    };
}

const AlertBar = connect(
    mapStateToProps,
    mapDispatchToProps
)(_AlertBar);

export default AlertBar;