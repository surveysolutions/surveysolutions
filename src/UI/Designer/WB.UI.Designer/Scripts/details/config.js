define('config',
    ['pnotify', 'ko', 'amplify'],
    function (toastr, ko, amplify) {

        var // properties
            //-----------------
            questionTypes = {
                "SingleOption": "SingleOption",
                "MultyOption": "MultyOption",
                "Numeric": "Numeric",
                "DateTime": "DateTime",
                "Text": "Text",
                "AutoPropagate": "AutoPropagate",
                "GpsCoordinates": "GpsCoordinates"
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
                createNumericQuestion: "AddNumericQuestion",
                cloneQuestion: "CloneQuestion",
                cloneNumericQuestion: "CloneNumericQuestion",
                updateQuestion: "UpdateQuestion",
                updateNumericQuestion: "UpdateNumericQuestion",
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
                    message: "Pre-filled questions can't be conditionally enabled. Would you like to erase the condition expression?",
                    okBtn: "Yes, erase the condition",
                    cancelBtn: "No, keep the condition"
                },
                weWillClearConditionAndValidation: {
                    message: "Questions filled in by the supervisor can't be conditionally enabled and don't support validation. Would you like to erase the condition and validation expressions?",
                    okBtn: "Yes, erase the expressions",
                    cancelBtn: "No, keep the expressions"
                },
                weWillClearSupervisorFlag: {
                    message: "If a question is pre-filled, it can't at the same time be marked as answered by the supervisor. Would you like to disable the 'answered by the supervisor' option for this question?",
                    okBtn: "Yes, disable it",
                    cancelBtn: "No, don't disable it"
                },
                weWillClearHeadFlag:{
                    message: "Questions answered by supervisor can't serve as a header of a roster group. Would you like to disable the 'head' option for this question?",
                    okBtn: "Yes, disable it",
                    cancelBtn: "No, don't disable it"
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
