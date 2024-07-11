import { defineRule } from 'vee-validate';
import { required, integer } from '@vee-validate/rules';

defineRule('required', required);
defineRule('integer', integer);

/*import { forEach, isEmpty } from 'lodash';
import { optionsParseRegex } from './helper';

defineRule('stringOptions', value => {
    if (!isEmpty(value)) {
        var options = (value || '').split('\n');
        var matchPattern = true;
        var invalidLines = [];
        forEach(options, function(option, index) {
            var currentLineValidationResult = optionsParseRegex.test(
                option || ''
            );
            matchPattern = matchPattern && currentLineValidationResult;
            if (!currentLineValidationResult) invalidLines.push(index + 1);
        });
        //return { valid: matchPattern, data: invalidLines };
        return (
            "You entered an invalid input. Each line should follow the format: 'Title...Value[...Attachment name]'. 'Value' must be an integer number. 'Title' must be an alpha-numeric string. 'Attachment name' is optional. No empty lines are allowed. Lines: " +
            invalidLines +
            '.'
        );
    }
    return true;
});*/
