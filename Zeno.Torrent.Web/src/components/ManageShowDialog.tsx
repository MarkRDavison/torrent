import { Button, Dialog, DialogActions, DialogContent, DialogContentText, DialogTitle } from "@material-ui/core";
import React from "react";
import { Show } from "../models/Show";

interface Props {
    open: boolean
    onClose: () => void
    onRemove: (id: string) => void
    clearCache: (id: string) => void
    show: Show | undefined
}

const _ManageShowDialog = (props: Props): JSX.Element => {

    return (
        <Dialog 
            open={props.open}
            onClose={props.onClose}>
            <DialogTitle>Manage Show</DialogTitle>
            <DialogContent>                
                <DialogContentText>{props.show?.name} ({props.show?.quality})</DialogContentText>
            </DialogContent>
            <DialogActions>
                <Button color="secondary" onClick={() => props.onClose()}>
                Cancel
                </Button>
                <Button color="primary" onClick={() => {
                    // eslint-disable-next-line no-restricted-globals
                    if (props.show && confirm(`Are you sure you want to Clear Cache for ${props.show.name}?`)) {
                        props.onClose();
                        props.clearCache(props.show.id);
                    }
                }}>
                Clear Cache
                </Button>
                <Button color="primary" onClick={() => {
                    // eslint-disable-next-line no-restricted-globals
                    if (props.show && confirm(`Are you sure you want to remove ${props.show.name}?`)) {
                        props.onClose();
                        props.onRemove(props.show.id);
                    }
                }}>
                Remove
                </Button>
            </DialogActions>
        </Dialog>
    );
};

const ManageShowDialog = _ManageShowDialog;
export default ManageShowDialog;