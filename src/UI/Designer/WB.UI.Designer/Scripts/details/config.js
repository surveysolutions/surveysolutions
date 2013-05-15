define('config',
    ['pnotify', 'ko', 'amplify'],
    function (toastr, ko, amplify) {

        var// properties
            //-----------------
            questionTypes = {
                "SingleOption": "SingleOption",
                "MultyOption": "MultyOption",
                "Numeric": "Numeric",
                "DateTime": "DateTime",
                "Text": "Text",
                "AutoPropagate": "AutoPropagate"
            },
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
                "",
                "AutoPropagated"
            ],
            commands = {
                createGroup: "AddGroup",
                cloneGroup: "CloneGroup",
                updateGroup: "UpdateGroup",
                deleteGroup: "DeleteGroup",
                groupMove: "MoveGroup",
                createQuestion: "AddQuestion",
                cloneQuestion : "CloneQuestion",
                updateQuestion: "UpdateQuestion",
                deleteQuestion: "DeleteQuestion",
                questionMove: "MoveQuestion"
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
            logger = $.pnotify, // use pnotify for the logger

            storeExpirationMs = (1000 * 60 * 60 * 24), // 1 day
            throttle = 400,
            title = 'Details',
            loggerTmeout = 2000,

            warnings = {
                propagatedGroupCantBecomeChapter: {
                    title: 'Cant move',
                    text: "Auto propagate group can't become a chapter"
                },
                cantMoveQuestionOutsideGroup: {
                    title: 'Cant move',
                    text: "You can't move question outside any group"
                },
                cantMoveGroupIntoPropagatedGroup: {
                    title: 'Cant move',
                    text: "You can't move group into propagated group"
                },
                cantMoveUnsavedItem: {
                    title: 'Cant move',
                    text: "You can't move unsaved items"
                },
                saveParentFirst: {
                    title: 'Cant save',
                    text: "Save parent item first"
                },
                savedData: 'Data saved successfully'
            },

            // methods
            //-----------------

            init = function () {
                logger.defaults.delay = loggerTmeout;

                ko.validation.configure({
                    messagesOnModified: true,
                    parseInputAttributes: true,
                    insertMessages: true,
                    decorateElement: true,
                    errorElementClass: 'error',
                    errorMessageClass: "help-inline"
                });

                ko.bindingHandlers.sortable.options = { cursor: "move", handle: ".handler", axis: "y", placeholder: "ui-state-highlight" };
                ko.bindingHandlers.draggable.options = { cursor: "move", handle: ".handler", axis: "y" };

            };

        init();

        return {
            logger: logger,
            storeExpirationMs: storeExpirationMs,
            throttle: throttle,
            title: title,
            warnings: warnings,
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
