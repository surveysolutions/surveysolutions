define('config',
    ['toastr', 'ko'],
    function (toastr, ko) {

        var
            // properties
            //-----------------
            hashes = {
                details: '#/details',
                detailsMenu: '#/details/group',
            },
            viewIds = {
                details: '#stacks'
            },
            messages = {
                viewModelActivated: 'viewmodel-activation'
            },
            stateKeys = {
                lastView: 'state.active-hash'
            },
            logger = toastr, // use toastr for the logger

            storeExpirationMs = (1000 * 60 * 60 * 24), // 1 day
            throttle = 400,
            title = 'Details',
            toastrTimeout = 2000,

            toasts = {
                changesPending: 'Please save or cancel your changes before leaving the page.',
                errorSavingData: 'Data could not be saved. Please check the logs.',
                errorGettingData: 'Could not retrieve data.  Please check the logs.',
                invalidRoute: 'Cannot navigate. Invalid route',
                retreivedData: 'Data retrieved successfully',
                savedData: 'Data saved successfully'
            },

            // methods
            //-----------------

            init = function () {
                toastr.options.timeOut = toastrTimeout;
            };

        init();

        return {
            logger: logger,
            storeExpirationMs: storeExpirationMs,
            throttle: throttle,
            title: title,
            toasts: toasts,
            hashes: hashes,
            viewIds: viewIds,
            messages: messages,
            stateKeys: stateKeys,
            window: window
        };
    });
