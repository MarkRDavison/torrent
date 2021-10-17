import { CircularProgress, IconButton, Table, TableBody, TableCell, TableHead, TableRow, Tooltip } from "@material-ui/core";
import axios from "axios";
import React, { useEffect, useState } from "react";
import { connect } from "react-redux";
import { Show } from "../models/Show";
import { clearAlert, setErrorAlert } from "../store/AlertReducer";
import { setFetchingShows, setShowsFetched, ShowsState } from "../store/ShowReducer";
import { DispatchType, RootState } from "../store/Store";
import config from "../util/config";
import AddShowDialog from "./AddShowDialog";
import BuildIcon from '@material-ui/icons/Build';
import ManageShowDialog from "./ManageShowDialog";

interface StateProps {
    showState: ShowsState
}

interface DispatchProps {
    beginFetchShows: () => void
    setShowsFetched: (shows: Show[], error: string | null) => void
}

type Props = StateProps & DispatchProps;

const _Shows = (props: Props): JSX.Element => {
    const [open, setOpen] = useState(false);
    const {
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
    const [activeManagedShowId, setActiveManagedShowId] = useState('');
    const [manageOpen, setManageOpen] = useState(false);

    const onManage = (show: Show) => {
        setActiveManagedShowId(show.id);
        setManageOpen(true);
    };

    const createRow = (show: Show): JSX.Element  => {
        return (
            <TableRow key={show.id}>
                <TableCell component='th' scope='row' width='40px'>
                    <Tooltip title='Manage show'>
                        <IconButton onClick={() => onManage(show)}>
                            <BuildIcon fontSize='small'/>
                        </IconButton>
                    </Tooltip>
                </TableCell>
                <TableCell component='th' scope='row'>{show.name}</TableCell>
                <TableCell component='th' scope='row'>{show.quality}</TableCell>
            </TableRow>
        );
    };

    const table = (props: StateProps): JSX.Element => {
        const shows = props.showState.shows;
        return (
            <div style={{ height: 600 }}>
                <Table>
                    <TableHead>
                        <TableRow>
                            <TableCell />
                            <TableCell>Name</TableCell>
                            <TableCell>Quality</TableCell>
                        </TableRow>
                    </TableHead>
                    <TableBody>
                        {shows.map(createRow)}
                    </TableBody>
                </Table>
            </div>
        );
    };

    const activeManagedShow = props.showState.shows.find(s => s.id === activeManagedShowId);

    const onRemoveShow = (id: string) => {
        axios.delete(`${config.bff_base_uri}/api/show/${id}`, {
            withCredentials: true
        });
    };

    const onClearCache = (id: string) => {
        axios.delete(`${config.bff_base_uri}/api/show/${id}/episode/all`, {
            withCredentials: true
        });
    }

    return (
        <div>
            <h1>SHOWS</h1>
            <button onClick={() => setOpen(true)}>ADD SHOW</button>
            <ManageShowDialog
                open={manageOpen}
                onClose={() => setManageOpen(false)}
                onRemove={onRemoveShow}
                clearCache={onClearCache}
                show={activeManagedShow}/>
            <AddShowDialog 
                open={open}
                onClose={() => setOpen(false)} />
            {props.showState.fetching && <CircularProgress color="inherit" />}
            {!props.showState.fetching && table(props)}
        </div>
    );
};

const mapStateToProps = (state: RootState): StateProps => {
    return {
        showState: state.showState
    };
}

const mapDispatchToProps = (dispatch: DispatchType): DispatchProps => {
    return {
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
}

const Shows = connect(
    mapStateToProps,
    mapDispatchToProps
)(_Shows);

export default Shows;