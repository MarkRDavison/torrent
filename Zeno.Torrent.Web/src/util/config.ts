export interface Config {
    dashboardRuntimeDataPollRate: number
    dashboardTorrentPollRate: number
    bff_base_uri: string
    login_uri: string
    logout_uri: string
}

declare global {
    interface Window {
        ENV: {
            ZENO_TORRENT_DAEMON_DASHBOARD_RUNTIME_DATA_POLL_RATE: number,
            ZENO_TORRENT_DAEMON_DASHBOARD_TORRENT_POLL_RATE: number
            ZENO_TORRENT_DAEMON_BFF_BASE_URI: string
            ZENO_TORRENT_DAEMON_LOGIN_URI: string
            ZENO_TORRENT_DAEMON_LOGOUT_URI: string
        };
    }
}

const createConfig = (): Config => {
    return {
        dashboardRuntimeDataPollRate: window.ENV.ZENO_TORRENT_DAEMON_DASHBOARD_RUNTIME_DATA_POLL_RATE ?? 5000,
        dashboardTorrentPollRate: window.ENV.ZENO_TORRENT_DAEMON_DASHBOARD_TORRENT_POLL_RATE ?? 60000,
        bff_base_uri: window.ENV.ZENO_TORRENT_DAEMON_BFF_BASE_URI ?? 'http://localhost:4000',
        login_uri: window.ENV.ZENO_TORRENT_DAEMON_LOGIN_URI ?? 'http://localhost:4000/auth/login',
        logout_uri: window.ENV.ZENO_TORRENT_DAEMON_LOGOUT_URI ?? 'http://localhost:4000/auth/logout'
    }
};

const config = createConfig();

export default config;