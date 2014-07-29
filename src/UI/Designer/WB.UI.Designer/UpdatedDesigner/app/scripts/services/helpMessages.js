angular.module('designerApp')
    .factory('helpService', [
        function() {
            var helpService = {
                'title': 'Title',
                'description': 'Description',
                'conditionExpression': 'A logical expression that activates(disactivates) the current question(group) depending on the answers on the other questions.',
                'validationExpression': 'A logical expression that validates an answer to the current question. Might include values of other questions.',
                'validationMessage': 'Validation message is shown when an answer to the current question fails a validation condition.',
                'instruction': 'Instruction',
                'mask': 'Mask',
                'rosterType': 'Roster Type',
                'variableName': 'Variable Name',
                'sourceQuestion': 'Source question (answer generates number of rows)',
                'titleQuestion': 'Titles of rows',
                'fixedTitles': 'Number of items determines the roster size. Each element of the list corresponds to a title of the roster row. Each element of the list should be placed on a separate line.',
                'questionType': 'Question Type',
                'variable': 'Variable',
                'variableLabel': 'Variable Label'
            };

            helpService.getHelpMessage = function(key) {
                return helpService[key];
            };

            return helpService;
        }
    ]);