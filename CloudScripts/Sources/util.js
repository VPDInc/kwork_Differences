let cachedData;
// Envinroment for working with cache.
const cacheUtil = {
    
    getCachedData() {
        try {
            return JSON.parse(server.GetTitleData({ key: constants.CACHED_VALUE }).Data[constants.CACHED_VALUE]);
        } catch (e) {
            return null;
        }
    },

    cachedConfig() {
        cachedData = this.getCachedData();
        return cachedData;
    },
}
