class ServerResponce {
    constructor(status, message, playerStateUpdate) {
        this.success = status,
        this.message = message,
        this.playerStateUpdate = playerStateUpdate
    }
}