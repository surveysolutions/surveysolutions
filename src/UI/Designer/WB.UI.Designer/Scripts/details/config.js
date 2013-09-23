define('config',
    ['pnotify', 'ko', 'amplify'],
    function(toastr, ko, amplify) {

        var // properties
            //-----------------
            questionTypes = {
                "SingleOption": "Categorical: one answer",
                "MultyOption": "Categorical: multiple answers",
                "Numeric": "Numeric",
                "DateTime": "DateTime",
                "Text": "Text",
                "AutoPropagate": "AutoPropagate",
                "GpsCoordinates": "Geo Location"
            },
            questionTypeOptions = [
                {
                    key: "SingleOption",
                    value: "Categorical: one answer"
                },
                {
                    key: "MultyOption",
                    value: "Categorical: multiple answers"
                },
                {
                    key: "Numeric",
                    value: "Numeric"
                },
                {
                    key: "DateTime",
                    value: "Date"
                },
                {
                    key: "Text",
                    value: "Text"
                },
                {
                    key: "AutoPropagate",
                    value: "Auto propagate"
                },
                {
                    key: "GpsCoordinates",
                    value: "Geo Location"
                }
            ],
            questionScopes = [
                "Interviewer",
                "Supervisor",
                "Headquarter"
            ],
            answerOrders = [
                {
                    key: "AsIs",
                    value: "Nothing"
                },
                {
                    key: "AZ",
                    value: "Label: A to Z"
                },
                {
                    key: "ZA",
                    value: "Label: Z to A"
                },
                {
                    key: "MinMax",
                    value: "Value: ascending"
                },
                {
                    key: "MaxMin",
                    value: "Value: descending"
                }
            ],
            groupTypes = [
                "None",
                "",
                "AutoPropagated"
            ],
            commands = {
                updateQuestionnaire: "UpdateQuestionnaire",
                createGroup: "AddGroup",
                cloneGroup: "CloneGroupWithoutChildren",
                updateGroup: "UpdateGroup",
                deleteGroup: "DeleteGroup",
                groupMove: "MoveGroup",
                createQuestion: "AddQuestion",
                cloneQuestion: "CloneQuestion",
                updateQuestion: "UpdateQuestion",
                deleteQuestion: "DeleteQuestion",
                questionMove: "MoveQuestion",
                addSharedPersonToQuestionnaire: "AddSharedPersonToQuestionnaire",
                removeSharedPersonFromQuestionnaire: "RemoveSharedPersonFromQuestionnaire"
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
                    content: "You should save this group to perfome some actions with it",
                    placement: "top",
                    trigger: "hover"
                }
            },
            logger = $.pnotify, // use pnotify for the logger

            storeExpirationMs = (1000 * 60 * 60 * 24), // 1 day
            throttle = 400,
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
                cantMoveIntoUnsavedItem: {
                    title: 'Cant move',
                    text: "You can't move items to unsaved groups or chapters"
                },
                saveParentFirst: {
                    title: 'Cant save',
                    text: "Save parent item first"
                },
                cantMoveAutoPropagatedGroupOutsideGroup: {
                    title: 'Cant move group',
                    text: "You can't move AutoPropagate group outside any chapter"
                },
                cantMoveFeaturedQuestionIntoAutoGroup: {
                    title: 'Cant move question',
                    text: "You can't move featured question into propagated group"
                },
                cantMoveAutoQuestionIntoAutoGroup: {
                    title: 'Cant move question',
                    text: "You can't move AutoPropagate question into propagated group"
                },
                cantMoveHeadQuestionOutsideAutoGroup: {
                    title: 'Cant move question',
                    text: "You can't move head question outside any propagated group"
                },
                savedData: 'Data saved successfully',
                weWillClearCondition: {
                    message: "Featured question can't have condition expression. We can clear condition and make this questuion featured.",
                    okBtn: "Yes, clear condition",
                    cancelBtn: "No, do not clear condition"
    }
            },
            // methods
            //-----------------

            init = function() {
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
            warnings: warnings,
            hashes: hashes,
            viewIds: viewIds,
            messages: messages,
            stateKeys: stateKeys,
            questionTypes: questionTypes,
            questionTypeOptions: questionTypeOptions,
            questionScopes: questionScopes,
            answerOrders: answerOrders,
            groupTypes: groupTypes,
            commands: commands,
            tips: tips
        };
    });
