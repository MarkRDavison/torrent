import { Action, applyMiddleware, combineReducers, createStore, Dispatch, Store } from "redux";
import { composeWithDevTools } from "redux-devtools-extension";
import thunk from "redux-thunk";
import { alertReducer } from "./AlertReducer";
import { showReducer } from "./ShowReducer";
import { torrentReducer } from "./TorrentReducer";
import { torrentRuntimeReducer } from "./TorrentRuntimeReducer";

const rootReducer = combineReducers({
    torrentState: torrentReducer,
    torrentRuntimeState: torrentRuntimeReducer,
    showState: showReducer,
    alertState: alertReducer
});

export type RootState = ReturnType<typeof rootReducer>;
export type GetStateType = () => RootState;
export type DispatchType = Dispatch<Action<unknown>>;

const store: Store = createStore(
    rootReducer,
    composeWithDevTools(
        applyMiddleware(thunk)
    )
);

export default store;