const constants = {
    CACHED_VALUE: "Config"
};

// Freeze the enums from being changed during operations
Object.values(constants).forEach(Object.freeze);

// Stop the enums object itself from being extended or having properties modified.
Object.freeze(constants);