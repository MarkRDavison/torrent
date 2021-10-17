import React, {useEffect, useState } from "react";
import { connect } from "react-redux";
import { DispatchType, RootState } from "../store/Store";
import { setFetchingTorrents, setTorrentRemoved, setTorrentsFetched, TorrentsState } from "../store/TorrentReducer";
import { IconButton, LinearProgress, Table, TableBody, TableCell, TableHead, TableRow, Button, Tooltip } from "@material-ui/core";
import { Torrent } from "../models/Torrent";
import { clearAlert, setErrorAlert } from "../store/AlertReducer";
import config from "../util/config";
import axios from "axios";
import { setFetchingTorrentRuntimeDatas, setTorrentRuntimeDatasFetched, TorrentRuntimeDatasState } from "../store/TorrentRuntimeReducer";
import { TorrentRuntimeData } from "../models/TorrentRuntimeData";
import ManageTorrentDialog from "./ManageTorrentDialog";
import BuildIcon from '@material-ui/icons/Build';
import AddMovieDialog from "./AddMovieDialog";
import AddTVDialog from "./AddTVDialog";

interface StateProps {
    torrentState: TorrentsState
    torrentRuntimeState: TorrentRuntimeDatasState
}
interface DispatchProps {
    beginFetchTorrents: () => void
    beginFetchTorrentRuntimeData: () => void
    setTorrentsFetched: (torrents: Torrent[], error: string | null) => void
    setTorrentRuntimeDataFetched: (trd: TorrentRuntimeData[], error: string | null) => void
    setTorrentRemove: (id: string) => void
}
type Props = StateProps & DispatchProps;

const getTypeDisplay = (downloadType: string): string => {
    switch (downloadType) {
        case 'MOV':
            return 'Movie';
        case 'SEA':
            return 'Season';
        case 'EP':
            return 'Episode';
        default:
            return downloadType;
    }
};

const getStateDisplay = (downloadType: string): string => {
    switch (downloadType) {
        case 'ADD':
            return 'Added';
        case 'INIT':
            return 'Initialising';
        case 'DOWN':
            return 'Downloading';
        case 'COMP':
            return 'Completed';
        case 'PROC':
            return 'Processed';
        default:
            return downloadType;
    }
};

const getHumanSizeFromBytes = (bytes?: number | null): string => {
    if (bytes === null || bytes === undefined){
        return '0';
    }
    const kb = bytes / 1024;
    if (kb < 1024) {
        return (Math.round(kb * 100) / 100).toString() + 'KiB';
    }
    const mb = kb / 1024;
    if (mb < 1024) {
        return (Math.round(mb * 100) / 100).toString() + 'MiB';
    }
    const gb = mb /1024;
    return (Math.round(gb * 100) / 100).toString() + 'GiB';
}

const createRow = (onManage: (t: Torrent, trd?: TorrentRuntimeData) => void, t: Torrent, trd?: TorrentRuntimeData): JSX.Element => {

    const createProgress = (t: Torrent, trd?: TorrentRuntimeData) => {
        let progress = 100.00;
        if (t.state !== 'PROC') {
            progress = Number.parseFloat((Math.round((trd?.percentage ?? 0) * 100) / 100).toFixed(2));
        }
        return (<TableCell component='th' scope='row'><LinearProgress value={progress} variant='determinate'></LinearProgress>{progress}%</TableCell>);
    };

    const getRemaining = (trd?: TorrentRuntimeData): string => {
        if (trd === null ||
            trd === undefined) {
            return '';
        }

        const remainingSize = ((100 - trd.percentage) / 100 * trd.size);
        const seconds = remainingSize / trd.downloadSpeed;

        if (seconds < 60){
            return seconds.toFixed(0) + 's';
        }

        const minutes = seconds / 60;
        if (minutes < 60){
            return minutes.toFixed(2) + 'm';
        }

        const hours = minutes / 60;
        if (hours < 24){
            return hours.toFixed(2) + 'h';
        }

        const days = hours / 24;
        return days.toFixed(2) + 'd';
    }

    return (        
        <TableRow key={t.id}>
            <TableCell component='th' scope='row' width='40px'>
                <Tooltip title='Manage torrent'>
                    <IconButton onClick={() => onManage(t, trd)}>
                        <BuildIcon fontSize='small'/>
                    </IconButton>
                </Tooltip>
            </TableCell>
            <TableCell component='th' scope='row'>{t.name}</TableCell>
            <TableCell component='th' scope='row'>{getTypeDisplay(t.downloadType)}</TableCell>
            {createProgress(t, trd)}
            <TableCell component='th' scope='row'>{getStateDisplay(trd?.state || t.state)}</TableCell>
            <TableCell component='th' scope='row'>{getHumanSizeFromBytes(trd?.size)} - {getHumanSizeFromBytes(trd?.downloadSpeed)}/s</TableCell>
            <TableCell component='th' scope='row'>{getRemaining(trd)}</TableCell>
        </TableRow>
    );
}

const _Dashboard = (props: Props): JSX.Element => {
    const [openMovie, setOpenMovie] = useState(false);
    const [openTV, setOpenTV] = useState(false);
    const [activeManagedTorrentId, setActiveManagedTorrentId] = useState('');
    const [manageOpen, setManageOpen] = useState(false);


    const { 
        beginFetchTorrents,
        setTorrentsFetched,
        beginFetchTorrentRuntimeData,
        setTorrentRuntimeDataFetched
    } = props;

    useEffect(() => {
        const poll = () => {
            beginFetchTorrents();
            axios
                .get(`${config.bff_base_uri}/api/download`, {
                    withCredentials: true
                })
                .then(response => setTorrentsFetched(response.data, null))
                .catch(error => setTorrentsFetched([], error));
        };

        poll();
        setInterval(poll, config.dashboardTorrentPollRate)
    }, [
        beginFetchTorrents,
        setTorrentsFetched
    ]);

    useEffect(() => {
        const poll = () => {
            beginFetchTorrentRuntimeData();
            axios
                .get(`${config.bff_base_uri}/api/download/all/state`, {
                    withCredentials: true
                })
                .then(response => setTorrentRuntimeDataFetched(response.data, null))
                .catch(error => setTorrentRuntimeDataFetched([], error));
        };

        poll();
        setInterval(poll, config.dashboardRuntimeDataPollRate);
    }, [
        beginFetchTorrentRuntimeData,
        setTorrentRuntimeDataFetched
    ]);

    const onManage = (t: Torrent, trd?: TorrentRuntimeData) => {
        setActiveManagedTorrentId(t.id);
        setManageOpen(true);
    };

    const table = (props: StateProps): JSX.Element => {
        const torrents = props.torrentState.torrents;
        const torrentRuntimeState = props.torrentRuntimeState.torrentRuntimeData;
        return (            
            <div style={{ height: 600 }}>
                <Table>
                    <TableHead>
                        <TableRow>
                            <TableCell/>
                            <TableCell>Name</TableCell>
                            <TableCell>Type</TableCell>
                            <TableCell>Progress</TableCell>
                            <TableCell>State</TableCell>
                            <TableCell>Speed</TableCell>
                            <TableCell>Remaining</TableCell>
                        </TableRow>
                    </TableHead>
                    <TableBody>
                        {torrents.map(t => createRow(onManage, t, torrentRuntimeState.find(trs => trs.id === t.id)))}
                    </TableBody>
                </Table>
            </div>
        );
    };

    const activeManagedTorrent = props.torrentState.torrents.find(t => t.id === activeManagedTorrentId);
    const activeManagedTRD = props.torrentRuntimeState.torrentRuntimeData.find(trd => trd.id === activeManagedTorrentId);

    const onRemoveTorrent = (id: string) => {
        axios.delete(`${config.bff_base_uri}/api/download/${id}`, {
            withCredentials: true
        });
    };

    return (
        <div>
            <h1>DASHBOARD</h1>
            <div>
                <Button variant='contained' onClick={() => setOpenMovie(true)}>ADD MOVIE</Button> <Button variant='contained' onClick={() => setOpenTV(true)}>ADD TV</Button>
                <ManageTorrentDialog
                    open={manageOpen}
                    onClose={() => setManageOpen(false)}
                    onRemove={onRemoveTorrent}
                    torrent={activeManagedTorrent}
                    trd={activeManagedTRD} />
                <AddMovieDialog
                    open={openMovie}
                    onClose={() => setOpenMovie(false)} />
                <AddTVDialog 
                    open={openTV}
                    onClose={() => setOpenTV(false)} />
            </div>
            {table(props)}
        </div>
    );
};

const mapStateToProps = (state: RootState): StateProps => {
    return {
        torrentState: state.torrentState,
        torrentRuntimeState: state.torrentRuntimeState
    };
};

const mapDispatchToProps = (dispatch: DispatchType): DispatchProps => {
    return {
        beginFetchTorrents: () => dispatch(setFetchingTorrents()),
        setTorrentsFetched: (torrents: Torrent[], error: string | null) => {
            if (error && error.length > 0) {
                dispatch(setErrorAlert(error));
                dispatch(setTorrentsFetched([]));
            } else {
                dispatch(clearAlert());
                dispatch(setTorrentsFetched(torrents));
            }
        },
        beginFetchTorrentRuntimeData: () => dispatch(setFetchingTorrentRuntimeDatas()),
        setTorrentRuntimeDataFetched: (trd: TorrentRuntimeData[], error: string | null) => {
            if (error && error.length > 0) {
                dispatch(setErrorAlert(error));
                dispatch(setTorrentRuntimeDatasFetched([]));
            } else {
                dispatch(clearAlert());
                dispatch(setTorrentRuntimeDatasFetched(trd));
            }
        },
        setTorrentRemove: (id: string) => dispatch(setTorrentRemoved(id))
    };
};

const Dashboard = connect(
    mapStateToProps,
    mapDispatchToProps
)(_Dashboard);
export default Dashboard;