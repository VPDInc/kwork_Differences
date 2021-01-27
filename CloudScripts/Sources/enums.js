const enums = {

    /**
     * Responce statuces.
     * @readonly
     */
    RESPONCE_STATUCES: {
        error: false,
        success: true,
    },

    /**
     * Server messages.
     * @readonly
     */
    MESSAGES: {
        LoginPlayerSucessfully: "Gift was claim successfully"
    }
};

// Freeze the enums from being changed during operations
Object.values(enums).forEach(Object.freeze);

// Stop the enums object itself from being extended or having properties modified.
Object.freeze(enums);