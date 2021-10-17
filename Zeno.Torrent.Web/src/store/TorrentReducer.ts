import { Torrent } from "../models/Torrent";

const SET_FETCHING_TORRENTS = 'SET_FETCHING_TORRENTS';
const SET_TORRENTS_FETCHED = 'SET_TORRENTS_FETCHED';
const SET_TORRENT_REMOVED = 'SET_TORRENT_REMOVED';
const SET_TORRENTS_FETCH_ERROR = 'SET_TORRENTS_FETCH_ERROR';

export interface TorrentsState {
    torrents: Torrent[]
    fetching: boolean
    error: string | undefined
}

const initialState: TorrentsState = {
    torrents: [],
    fetching: false,
    error: undefined
}

interface FetchingTorrentsAction {
    type: typeof SET_FETCHING_TORRENTS
}

interface TorrentsFetchedAction {
    type: typeof SET_TORRENTS_FETCHED
    payload: Torrent[]
}

interface TorrentRemovedAction {
    type: typeof SET_TORRENT_REMOVED
    payload: string
}

interface TorrentFetchErrorAction {
    type: typeof SET_TORRENTS_FETCH_ERROR
    payload: string
}

export type TorrentActionTypes =
    FetchingTorrentsAction |
    TorrentsFetchedAction |
    TorrentRemovedAction |
    TorrentFetchErrorAction;

export function setFetchingTorrents(): TorrentActionTypes {
    return {
        type: SET_FETCHING_TORRENTS
    }
}
export function setTorrentsFetched(torrents: Torrent[]): TorrentActionTypes {
    return {
        type: SET_TORRENTS_FETCHED,
        payload: torrents
    }
}
export function setTorrentRemoved(id: string): TorrentActionTypes {
    return {
        type: SET_TORRENT_REMOVED,
        payload: id
    }
}
export function setTorrentsFetchError(message: string): TorrentActionTypes {
    return {
        type: SET_TORRENTS_FETCH_ERROR,
        payload: message
    }
}

export function torrentReducer(
    state = initialState,
    action: TorrentActionTypes
): TorrentsState {
    if (action === undefined || action === null) {
        return state;
    }

    switch (action.type) {
        case SET_FETCHING_TORRENTS:
            return {
                ...state,
                fetching: true,
                error: undefined
            }
        case SET_TORRENTS_FETCHED:
            return {
                ...state,
                torrents: action.payload,
                fetching: false,
                error: undefined
            }
        case SET_TORRENT_REMOVED:
            return {
                ...state,
                torrents: [...state.torrents.filter(t => t.id !== action.payload)],
                fetching: false,
                error: undefined
            }
        case SET_TORRENTS_FETCH_ERROR:
            return {
                ...state,
                fetching: false,
                error: action.payload
            }
    }

    /* istanbul ignore next */
    return state;
}