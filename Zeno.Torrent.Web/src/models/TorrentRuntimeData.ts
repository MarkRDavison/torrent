export interface PeerInfo {
    connectionString: string
    uploadSpeed: number
    updloadTotal: number
    downloadSpeed: number
    downloadTotal: number
    client: string
}

export interface TorrentRuntimeData {
    id: string
    percentage: number
    uploadSpeed: number
    downloadSpeed: number
    state: string
    peerInfo: PeerInfo[]
    size: number
}