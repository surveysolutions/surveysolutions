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
                'variableName': 'Variable names may be 1 to 32 characters long and must start with a-z, A-Z, or _, and the remaining characters may be a-z, A-Z, _, or 0-9.',
                'titleQuestion': 'Titles of rows',
                'variableLabel': 'A text up to 80 characters that will be attached to the exported variable',
                'conditionExpression': 'A logical expression that activates(deactivates) the current question(group) depending on the answers on the other questions.',
                'validationExpression': 'A logical expression that validates an answer to the current question. Might include values of other questions.',
                'validationMessage': 'Error message is shown when an answer to the current question fails a validation condition.',
                'sourceQuestion': 'Source question (answer generates number of rows)',
                'fixedTitles': 'Number of items determines the roster size. Each element of the list corresponds to a title of the roster row. Each element of the list should be placed on a separate line.'
            };

            helpService.getHelpMessage = function(key) {
                return helpService[key];
            };

            return helpService;
        }
    ]);