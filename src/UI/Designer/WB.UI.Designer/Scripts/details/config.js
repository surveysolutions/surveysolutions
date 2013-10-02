define('config',
    ['pnotify', 'ko', 'amplify'],
    function (toastr, ko, amplify) {

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
            questionScopes = {
                interviewer: "Interviewer",
                supervisor: "Supervisor",
                headquarters: "Headquarters"
            },
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
                    content: "You should save this group to perform any actions with it",
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
                    title: "Can't move",
                    text: "AutoPropagate group can't become a chapter"
                },
                cantMoveQuestionOutsideGroup: {
                    title: "Can't move",
                    text: "You can't move a question outside of any group"
                },
                cantMoveGroupIntoPropagatedGroup: {
                    title: "Can't move",
                    text: "You can't move a group into a propagated group"
                },
                cantMoveUnsavedItem: {
                    title: "Can't move",
                    text: "You can't move unsaved items"
                },
                cantMoveIntoUnsavedItem: {
                    title: "Can't move",
                    text: "You can't move items to unsaved groups or chapters"
                },
                saveParentFirst: {
                    title: "Can't move",
                    text: "Save the parent item first"
                },
                cantMoveAutoPropagatedGroupOutsideGroup: {
                    title: "Can't move group",
                    text: "You can't move an AutoPropagate group outside any chapter"
                },
                cantMoveFeaturedQuestionIntoAutoGroup: {
                    title: "Can't move question",
                    text: "You can't move a pre-filled question into a propagated group"
                },
                cantMoveAutoQuestionIntoAutoGroup: {
                    title: "Can't move question",

                    text: "You can't move an AutoPropagate question into a propagated group"
                },
                cantMoveHeadQuestionOutsideAutoGroup: {
                    title: "Can't move question",

                    text: "You can't move a head question outside of any propagated group"
                },
                savedData: 'Data saved successfully',
                weWillClearCondition: {
                    message: "Pre-filled question can't have condition expression. We can clear condition and make this questuion pre-filled.",
                    okBtn: "Yes, clear condition",
                    cancelBtn: "No, do not clear condition"
                },
                weWillClearConditionAndValidation: {
                    message: "Filled by supervisor questions can't have condition and validation expression. We can clear condition and validation and mark this questuion as filled by supervisor.",
                    okBtn: "Yes, clear condition and validation",
                    cancelBtn: "No, do not clear them"
                },
                weWillClearSupervisorFlag: {
                    message: "Pre-filled question can't be answered by supervisor. We will clear filled by supervisor flag to make this questuion pre-filled.",
                    okBtn: "Yes, clear filled by supervisor flag",
                    cancelBtn: "No, do not clear it"
                },
                weWillClearHeadFlag:{
                    message: "Filled by supervisor questions can't be head of group. We will clear head flag to mark this questuion as filled by supervisor.",
                    okBtn: "Yes, clear head flag",
                    cancelBtn: "No, do not clear it"
                },
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
