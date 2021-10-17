import { Show } from "../models/Show";

const SET_FETCHING_SHOWS = 'SET_FETCHING_SHOWS';
const SET_SHOWS_FETCHED = 'SET_SHOWS_FETCHED';
const SET_SHOW_ADDED = 'SET_SHOW_ADDED';
const SET_SHOWS_FETCH_ERROR = 'SET_SHOWS_FETCH_ERROR';

export interface ShowsState {
    shows: Show[]
    fetching: boolean
    error: string | undefined
}

const initialState: ShowsState = {
    shows: [],
    fetching: false,
    error: undefined
}

interface FetchingShowsAction {
    type: typeof SET_FETCHING_SHOWS
}

interface ShowsFetchedAction {
    type: typeof SET_SHOWS_FETCHED
    payload: Show[]
}

interface ShowAddedAction {
    type: typeof SET_SHOW_ADDED
    payload: Show
}

interface ShowFetchErrorAction {
    type: typeof SET_SHOWS_FETCH_ERROR
    payload: string
}

export type ShowActionTypes =
    FetchingShowsAction |
    ShowsFetchedAction |
    ShowAddedAction |
    ShowFetchErrorAction;

export function setFetchingShows(): ShowActionTypes {
    return {
        type: SET_FETCHING_SHOWS
    }
}
export function setShowsFetched(shows: Show[]): ShowActionTypes {
    return {
        type: SET_SHOWS_FETCHED,
        payload: shows
    }
}
export function setShowAdded(show: Show): ShowActionTypes {
    return {
        type: SET_SHOW_ADDED,
        payload: show
    }
}
export function setShowsFetchError(message: string): ShowActionTypes {
    return {
        type: SET_SHOWS_FETCH_ERROR,
        payload: message
    }
}

export function showReducer(
    state = initialState,
    action: ShowActionTypes
): ShowsState {
    if (action === undefined || action === null) {
        return state;
    }

    switch (action.type) {
        case SET_FETCHING_SHOWS:
            return {
                ...state,
                fetching: true,
                error: undefined
            }
        case SET_SHOWS_FETCHED:
            return {
                ...state,
                shows: action.payload,
                fetching: false,
                error: undefined
            }
        case SET_SHOW_ADDED:
            return {
                ...state,
                shows: state.shows.concat(action.payload),
                fetching: false,
                error: undefined
            }
        case SET_SHOWS_FETCH_ERROR:
            return {
                ...state,
                fetching: false,
                error: action.payload
            }
    }

    /* istanbul ignore next */
    return state;
}