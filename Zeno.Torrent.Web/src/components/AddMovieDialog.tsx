import React, { ChangeEvent, useState } from "react";
import { Button, Dialog, DialogActions, DialogContent, DialogContentText, DialogTitle, TextField } from "@material-ui/core";
import axios from "axios";
import config from "../util/config";
import { DispatchType, RootState } from "../store/Store";
import { connect } from "react-redux";
import { Torrent } from "../models/Torrent";
import { clearAlert, setErrorAlert } from "../store/AlertReducer";
import { setTorrentsFetched, TorrentsState } from "../store/TorrentReducer";

interface OwnProps {
    open: boolean
    onClose: () => void
}

interface StateProps {
    torrentState: TorrentsState
}

interface DispatchProps {
    setTorrentsFetched: (torrents: Torrent[], error: string | null) => void
}

type Props = OwnProps & StateProps & DispatchProps;

const _AddMovieDialog = (props: Props): JSX.Element => {
    const [magnet, setMagnet] = useState('');
    const handleChange = (event: ChangeEvent<HTMLInputElement>) => {
        event.preventDefault();
        setMagnet(event.target.value);
    };
    const {
        setTorrentsFetched
    } = props;

    return (
        <Dialog
            open={props.open}
            onClose={props.onClose}>
            <DialogTitle>Add Movie</DialogTitle>
            <DialogContent>
                <DialogContentText>
                    Paste the magnet link for the movie below
                </DialogContentText>
                <TextField
                    autoFocus
                    margin='dense'
                    name='magnet'
                    label='Magnet'
                    type='text'
                    fullWidth
                    onChange={handleChange}
                    value={magnet}/>
            </DialogContent>
            <DialogActions>
                <Button color="secondary" onClick={() => props.onClose()}>
                Cancel
                </Button>
                <Button color="primary" disabled={!(magnet.length > 0)} onClick={async () => {
                    const response = await axios
                        .post(`${config.bff_base_uri}/api/download`, {
                            OriginalUri: magnet,
                            DownloadType: "MOV"
                        }, {
                            withCredentials: true
                        });
                    setTorrentsFetched([response.data, ...props.torrentState.torrents], null);
                    props.onClose();
                    setMagnet('');
                }}>
                Add
                </Button>
            </DialogActions>
        </Dialog>
    );
};

const mapStateToProps = (state: RootState): StateProps => {
    return {
        torrentState: state.torrentState,
    };
};

const mapDispatchToProps = (dispatch: DispatchType): DispatchProps => {
    return {
        setTorrentsFetched: (torrents: Torrent[], error: string | null) => {
            if (error && error.length > 0) {
                dispatch(setErrorAlert(error));
                dispatch(setTorrentsFetched([]));
            } else {
                dispatch(clearAlert());
                dispatch(setTorrentsFetched(torrents));
            }
        }
    };
};

const AddMovieDialog = connect(
    mapStateToProps,
    mapDispatchToProps
)(_AddMovieDialog);
export default AddMovieDialog;