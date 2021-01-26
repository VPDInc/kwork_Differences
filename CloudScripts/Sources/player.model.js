/**
 * Login player
 * @param {Object} args has not have parameters
 * @returns {ServerResponce} server responce object.
 */
handlers.GetLoginData = (args) => {

    try {


    } catch (e) {
        return new ServerResponce(enums.RESPONCE_STATUCES.error, e.errorMessage);
    }

    return new ServerResponce(enums.RESPONCE_STATUCES.success, enums.MESSAGES.LoginPlayerSucessfully);
}