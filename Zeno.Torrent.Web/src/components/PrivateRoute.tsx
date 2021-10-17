import { CircularProgress } from "@material-ui/core";
import React from "react";
import { Route } from "react-router-dom";
import { useAuth } from "./AuthContext";

const PrivateRoute = (props: any): JSX.Element => {
    const { component, ...rest } = props;
    const { isLoggedIn, isLoggingIn, login } = useAuth();
    
    const renderFunction = (Component: any) => (props: any) => {    
        var ifValue = !!Component && (isLoggedIn || isLoggingIn);
        if (!isLoggedIn && isLoggingIn){            
            return <CircularProgress />;
        }
        if (ifValue) {
            return <Component {...props} />;
        }
        else {
            login();
            return <CircularProgress />;
        }
    }

    return <Route {...rest} render={renderFunction(component)} />;
};

export default PrivateRoute;