angular.module('' + 'designerApp')
    .factory('helpService', [
        function() {
            var helpService = {
                'instruction': 'Instruction button will be attached to the question that will show this text',
                'mask': 'Formatted entry for alpha-numerical values: phone numbers, ID codes, etc.\n' +
                    'Examples:"~" - Represents an alpha character (A-Z,a-z), ' +
                    '"#" - Represents a numeric character (0-9), ' +
                    '"*" - Represents an alphanumeric character (A-Z,a-z,0-9). ' +
                    '"##/##/####" - date, ' +
                    '"(###) ###-####" - phone number, ' +
                    '"AA####" - flight number operated by American AirLines or ' +
                    '"~*-###-~###"',
                'variableName': 'Variable names and roster IDs may be from 1 to 32 characters long and must contain only following characters: a-z, A-Z, _, or 0-9. The first character cannot be 0-9 or _ and the last one cannot be _.',
                'titleQuestion': 'Titles of rows',
                'variableLabel': 'A text up to 80 characters that will be attached to the exported variable',
                'conditionExpression': 'A logical expression that activates(deactivates) the current item depending on the answers on the other questions.',
                'validationExpression': 'A logical expression that validates an answer to the current question. Might include values of other questions.',
                'validationMessage': 'Error message is shown when an answer to the current question fails a validation condition.',
                'sourceQuestion': 'Source question (answer generates number of rows)',
                'fixedTitles': 'Number of items determines the roster size. Each element of the list corresponds to a title of the roster row. Each element of the list should be placed on a separate line.',
                'useFormatting': 'On interviewer app as user types this setting converts a number to a string of the form "-d,ddd,ddd.ddd…", where "-" indicates a negative number symbol if required, "d" indicates a digit (0-9), "," indicates a group separator, and "." indicates a decimal point symbol. Symbols may vary depending on tablet settings.'
            };

            helpService.getHelpMessage = function(key) {
                return helpService[key];
            };

            return helpService;
        }
    ]);