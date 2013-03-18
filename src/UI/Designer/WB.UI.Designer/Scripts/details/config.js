define('config',
    ['toastr', 'ko', 'amplify'],
    function (toastr, ko, amplify) {

        var// properties
            //-----------------
            questionTypes = [
                "SingleOption",
                "YesNo",
                "DropDownList",
                "MultyOption",
                "Numeric",
                "DateTime",
                "GpsCoordinates",
                "Text",
                "AutoPropagate"
            ],
            questionScopes = [
                "Interviewer",
                "Supervisor",
                "Headquarter"
            ],
            answerOrders = [
                "AsIs",
                "Random",
                "AZ",
                "ZA",
                "MinMax",
                "MaxMin"
            ],
            groupTypes = [
                "None",
                "Propagated",
                "AutoPropagated"
            ],
            commands = {
                createGroup: "CreateGroup",
                updateGroup: "UpdateGroup",
                deleteGroup: "DeleteGroup",
                moveGroup: "MoveGroup",
                createQuestion: "CreateQuestion",
                updateQuestion: "UpdateQuestion",
                deleteQuestion: "DeleteQuestion",
                moveQuestion: "MoveQuestion"
            },
            hashes = {
                details: '#/details',
                detailsGroup: '#/details/group',
                detailsQuestion: '#/details/question',
                detailsQuestionnaire: '#/details/questionnaire'
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
            tips = {
                newGroup: {
                    title: "Save this group",
                    content: "",
                    placement: "top",
                    trigger: "hover"
                }
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

                ko.validation.configure({
                    insertMessages: true,
                    decorateElement: true,
                    errorElementClass: 'error',
                    errorMessageClass: "help-inline"
                });
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
            questionTypes: questionTypes,
            questionScopes: questionScopes,
            answerOrders: answerOrders,
            groupTypes: groupTypes,
            commands: commands,
            tips: tips
        };
    });
