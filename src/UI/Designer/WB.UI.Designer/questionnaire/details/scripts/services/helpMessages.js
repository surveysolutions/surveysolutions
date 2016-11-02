angular.module('' + 'designerApp')
    .factory('helpService', [
        function() {
            var helpService = {
                instruction: 'Instruction text that will be attached to the question.',
                mask: 'Formatted entry for alpha-numerical values: phone numbers, ID codes, etc.\n' +
                    'Examples:"~" - Represents an alpha character (A-Z,a-z), ' +
                    '"#" - Represents a numeric character (0-9), ' +
                    '"*" - Represents an alphanumeric character (A-Z,a-z,0-9). ' +
                    '"##/##/####" - date, ' +
                    '"(###) ###-####" - phone number, ' +
                    '"AA####" - flight number operated by American AirLines or ' +
                    '"~*-###-~###"',
                variableName: 'Variable names and roster IDs may be from 1 to 32 characters long and must contain only following characters: a-z, A-Z, _, or 0-9. The first character cannot be 0-9 or _ and the last one cannot be _.',
                titleQuestion: 'Titles of rows',
                variableLabel: 'A text up to 80 characters that will be attached to the exported variable',
                conditionExpression: 'A logical expression that activates(deactivates) the current item depending on the answers on the other questions.',
                validationExpression: 'A logical expression that validates an answer to the current question. Might include values of other questions.',
                validationMessage: 'Error message is shown when an answer to the current question fails a validation condition.',
                sourceQuestion: 'Source question (answer generates number of rows)',
                expression: 'A logical expression that is calculated depending on the answers on the other questions.',
                fixedTitles: 'Number of items determines the roster size. Each element of the list corresponds to a title of the roster row. Each element of the list should be placed on a separate line.',
                useFormatting: 'When checked, the numeric field on the tablet will be formatted as: d,ddd,ddd.dd, "d" represents digits (0-9), “,”(comma) is a thousand separator, and "." (dot) is a decimal point symbol.  Thousand separator and the decimal point symbol may vary for tablets with different cultures.',
                hideIfDisabled: 'By default, a disabled item is greyed-out. Checking this option will hide a disabled question or section on a tablet.',
                hideInstructions: 'By default, an instruction text is shown on a tablet. Checking this option will show only the first line of instructions. Tapping on that line will show the complete text.',
                isTimestamp: 'This option allows the user to stamp the current time on tablet',
                variableDescription: 'Text that will be shown next to a variable for testing purposes'
            };

            helpService.getHelpMessage = function(key) {
                return helpService[key];
            };

            return helpService;
        }
    ]);