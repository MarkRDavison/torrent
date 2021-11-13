import { Button, CircularProgress, Dialog, DialogActions, DialogContent, DialogTitle, FormControlLabel, MenuItem, Radio, RadioGroup, Select, TextField } from "@material-ui/core";
import DialogContentText from "@material-ui/core/DialogContentText";
import axios from "axios";
import React, { ChangeEvent, useEffect, useState } from "react";
import { connect } from "react-redux";
import { Show } from "../models/Show";
import { Torrent } from "../models/Torrent";
import { clearAlert, setErrorAlert } from "../store/AlertReducer";
import { setFetchingShows, setShowsFetched, ShowsState } from "../store/ShowReducer";
import { DispatchType, RootState } from "../store/Store";
import { setTorrentsFetched, TorrentsState } from "../store/TorrentReducer";
import config from "../util/config";

interface OwnProps {
    open: boolean
    onClose: () => void
}

interface StateProps {
    torrentState: TorrentsState
    showState: ShowsState
}

interface DispatchProps {
    setTorrentsFetched: (torrents: Torrent[], error: string | null) => void
    beginFetchShows: () => void
    setShowsFetched: (shows: Show[], error: string | null) => void
}

interface FormData {
    magnet: string
    destinationTypeId: string
    showId: string
}

type Props = OwnProps & StateProps & DispatchProps;

function splitMulti(str: string, tokens: string[]): string[] {
    var tempChar = tokens[0]; // We can use the first token as a temporary join character
    for(var i = 1; i < tokens.length; i++){
        str = str.split(tokens[i]).join(tempChar);
    }
    return str.split(tempChar);
}


const _AddTVDialog = (props: Props): JSX.Element => {
    const initialData: FormData = {
        magnet: '',
        destinationTypeId: 'EP',
        showId: ''
    }
    
    const matchShow = (torrentName: string): Show | null => {
        const separators = [
            '+', '.', '%20'
        ];
        const torrentTokens = splitMulti(torrentName, separators);
        let matched: Show | null = null;
        


        props.showState.shows.forEach(s => {
            const showTokens = s.name.split(' ');            
            for (let torrentOffset = 0; torrentOffset < torrentTokens.length; ++torrentOffset) {
                let match = true;
                for (let i = 0; i < showTokens.length; ++i) {
                    if (showTokens[i].toUpperCase() !== torrentTokens[i + torrentOffset].toUpperCase()){
                        match = false;
                        break;
                    }
                }
                if (match) {
                    matched = s;
                    break;
                }
            }
        });

        return matched;
    }
    const [formData, setFormData] = useState<FormData>(initialData);
    const handleChange = (event: ChangeEvent<HTMLInputElement>) => {
        event.preventDefault();        
        const newFormData = {
            ...formData,
            [event.target.name]: event.target.value
        };
        let matched: Show | null = null;
        if (newFormData.magnet.length > 0) {
            const dnAttribute = newFormData.magnet.split('&').filter(s => s.startsWith('dn='));
            if (dnAttribute[0]) {
                const name = dnAttribute[0].slice(3);                
                matched = matchShow(name);
            }
        }
        newFormData.showId = matched?.id || '';
        setFormData(newFormData);
    };
    const handleSelectChanged = (event: React.ChangeEvent<{name?: string | undefined, value: unknown}>) => {
        setFormData({
            ...formData,
            showId: event.target.value as string || ''
        });
    }
    const {
        setTorrentsFetched,
        beginFetchShows,
        setShowsFetched
    } = props;
    useEffect(() => {
        beginFetchShows();
        axios
            .get(`${config.bff_base_uri}/api/show`, {
                withCredentials: true
            })    
            .then(response => setShowsFetched(response.data, null))
            .catch(error => setShowsFetched([], error));
    }, [
        beginFetchShows,
        setShowsFetched
    ]);

    return (
        <Dialog
            open={props.open}
            onClose={props.onClose}>
            <DialogTitle>Add TV</DialogTitle>
            {props.showState.fetching ?
            <CircularProgress color="inherit" /> :
            <DialogContent>
                <DialogContentText>
                    Paste the magnet link for the TV show below
                </DialogContentText>
                <TextField
                    autoFocus
                    margin='dense'
                    name='magnet'
                    label='Magnet'
                    type='text'
                    fullWidth
                    onChange={handleChange}
                    value={formData.magnet}/>
                <RadioGroup                    
                    onChange={handleChange}
                    value={formData.destinationTypeId}>
                    <FormControlLabel name='destinationTypeId' value="EP" control={<Radio />} label="Episode" />
                    <FormControlLabel name='destinationTypeId' value="SEA" control={<Radio />} label="Season" />
                </RadioGroup>
                <Select
                    name='showId'
                    fullWidth
                    value={formData.showId}
                    onChange={handleSelectChanged}>
                    {props.showState.shows.map(s => <MenuItem key={s.id} value={s.id}>{s.name}</MenuItem>)}
                </Select>
            </DialogContent>
            }
            <DialogActions>
                <Button color="secondary" onClick={() => props.onClose()}>
                Cancel
                </Button>
                <Button color="primary" disabled={formData.magnet.length <= 0 || formData.showId.length <= 0} onClick={async () => {
                    const response = await axios
                        .post(`${config.bff_base_uri}/api/download`, {
                            OriginalUri: formData.magnet,
                            DownloadType: formData.destinationTypeId,
                            DestinationTypeId: formData.showId
                        }, {
                            withCredentials: true
                        });
                    setTorrentsFetched([response.data, ...props.torrentState.torrents], null);
                    props.onClose();
                    setFormData(initialData);
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
        showState: state.showState
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
        },        
        beginFetchShows: () => dispatch(setFetchingShows()),
        setShowsFetched: (shows: Show[], error: string | null) => {
            if (error && error.length > 0) {
                dispatch(setErrorAlert(error));
                dispatch(setShowsFetched([]));
            } else {
                dispatch(clearAlert());
                dispatch(setShowsFetched(shows));
            }
        }
    };
};

const AddTVDialog = connect(
    mapStateToProps,
    mapDispatchToProps
)(_AddTVDialog);
export default AddTVDialog;