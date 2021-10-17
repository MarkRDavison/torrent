import { TorrentRuntimeData } from '../models/TorrentRuntimeData';

const SET_FETCHING_TORRENT_RUNTIME = 'SET_FETCHING_TORRENT_RUNTIME';
const SET_TORRENT_RUNTIME = 'SET_TORRENT_RUNTIME';
const SET_TORRENT_RUNTIME_FETCH_ERROR = 'SET_TORRENT_RUNTIME_FETCH_ERROR';

export interface TorrentRuntimeDatasState {
    torrentRuntimeData: TorrentRuntimeData[]
    fetching: boolean
    error: string | undefined
}

const initialState: TorrentRuntimeDatasState = {
    torrentRuntimeData: [],
    fetching: false,
    error: undefined
}

interface FetchingTorrentRuntimeDatasAction {
    type: typeof SET_FETCHING_TORRENT_RUNTIME
}

interface TorrentRuntimeDatasFetchedAction {
    type: typeof SET_TORRENT_RUNTIME
    payload: TorrentRuntimeData[]
}

interface TorrentRuntimeDataFetchErrorAction {
    type: typeof SET_TORRENT_RUNTIME_FETCH_ERROR
    payload: string
}

export type TorrentRuntimeDataActionTypes =
    FetchingTorrentRuntimeDatasAction |
    TorrentRuntimeDatasFetchedAction |
    TorrentRuntimeDataFetchErrorAction;

export function setFetchingTorrentRuntimeDatas(): TorrentRuntimeDataActionTypes {
    return {
        type: SET_FETCHING_TORRENT_RUNTIME
    }
}
export function setTorrentRuntimeDatasFetched(torrents: TorrentRuntimeData[]): TorrentRuntimeDataActionTypes {
    return {
        type: SET_TORRENT_RUNTIME,
        payload: torrents
    }
}
export function setTorrentRuntimeDatasFetchError(message: string): TorrentRuntimeDataActionTypes {
    return {
        type: SET_TORRENT_RUNTIME_FETCH_ERROR,
        payload: message
    }
}

export function torrentRuntimeReducer(
    state = initialState,
    action: TorrentRuntimeDataActionTypes
): TorrentRuntimeDatasState {
    if (action === undefined || action === null) {
        return state;
    }

    switch (action.type) {
        case SET_FETCHING_TORRENT_RUNTIME:
            return {
                ...state,
                fetching: true,
                error: undefined
            }
        case SET_TORRENT_RUNTIME:
            return {
                ...state,
                torrentRuntimeData: action.payload,
                fetching: false,
                error: undefined
            }
        case SET_TORRENT_RUNTIME_FETCH_ERROR:
            return {
                ...state,
                fetching: false,
                error: action.payload
            }
    }

    /* istanbul ignore next */
    return state;
}