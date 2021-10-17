import { Button, Dialog, DialogActions, DialogContent, DialogContentText, DialogTitle } from "@material-ui/core";
import React from "react";
import { Torrent } from "../models/Torrent";
import { TorrentRuntimeData } from "../models/TorrentRuntimeData";

interface Props {
    open: boolean
    onClose: () => void
    onRemove: (id: string) => void
    torrent: Torrent | undefined
    trd: TorrentRuntimeData | undefined
}

const _ManageTorrentDialog = (props: Props): JSX.Element => {

    return (
        <Dialog 
            open={props.open}
            onClose={props.onClose}>
            <DialogTitle>Manage Torrent</DialogTitle>
            <DialogContent>                
                <DialogContentText>{props.torrent?.name}</DialogContentText>
            </DialogContent>
            <DialogActions>
                <Button color="secondary" onClick={() => props.onClose()}>
                Cancel
                </Button>
                <Button color="primary" onClick={() => {
                    // eslint-disable-next-line no-restricted-globals
                    if (props.torrent && confirm(`Are you sure you want to remove ${props.torrent.name}?`)) {
                        props.onClose();
                        props.onRemove(props.torrent.id);
                    }
                }}>
                Remove
                </Button>
            </DialogActions>
        </Dialog>
    );
};

const ManageTorrentDialog = _ManageTorrentDialog;
export default ManageTorrentDialog;