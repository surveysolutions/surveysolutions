export function convertToText(items, isCascading) {
    var stringifiedOptions = '';
    const options = [...items];

    return new Promise(resolve => {
        let maxLength = null;

        options.forEach(i => {
            if (i != null && i.title != null) {
                if (maxLength == null || i.title.length > maxLength)
                    maxLength = i.title.length;
            }
        });

        maxLength += 3;

        var chunk = 2000;
        var index = 0;

        function doChunk() {
            var cnt = chunk;

            while (cnt-- && index < options.length) {
                // process array[index] here
                const option = options[index];

                if (option != null && option != '') {
                    stringifiedOptions +=
                        (option.title || '') +
                        Array(maxLength + 1 - (option.title || '').length).join(
                            '.'
                        );

                    if (isCascading)
                        stringifiedOptions +=
                            (option.parentValue === 0
                                ? '0'
                                : option.parentValue || '') + '/';
                    stringifiedOptions +=
                        option.value === 0 ? '0' : option.value || '';
                    stringifiedOptions += '\n';
                }

                ++index;
            }
            //resolve(stringifiedOptions.trim());
            if (index < options.length) {
                // set Timeout for async iteration
                setTimeout(doChunk, 90);
            }

            if (index >= options.length) {
                resolve(stringifiedOptions.trim());
            }
        }

        doChunk();
    });
}

export function convertToTable(stringified, isCascading) {
    const regex = isCascading
        ? /(.+?)[….\s]+([-+]?\d+)\/([-+]?\d+)\s*/
        : /(.+?)[….\s]+([-+]?\d+)\s*$/;

    var optionsStringList = (stringified || '').split('\n');

    optionsStringList = optionsStringList.filter(function(line) {
        return line.trim() != null && line.trim() != '';
    });

    var options = optionsStringList.map(function(item) {
        var matches = item.match(regex);
        if (!matches) {
            return {};
        }
        if (isCascading) {
            if (matches.length > 3) {
                return {
                    value: matches[3] * 1,
                    parentValue: matches[2] * 1,
                    title: matches[1]
                };
            } else
                return {
                    value: matches[2] * 1,
                    title: matches[1]
                };
        } else
            return {
                value: matches[2] * 1,
                title: matches[1]
            };
    });

    return options;
}

export function validateText(value, isCascading) {
    if (value == null || value === '') return [];
    var options = (value || '').split(/\r\n|\r|\n/);

    const regex = isCascading
        ? /(.+?)[….\s]+([-+]?\d+)\/([-+]?\d+)/
        : /(.+?)[….\s]+([-+]?\d+)/;

    const diff = [];
    for (var i = 0; i < options.length; i++) {
        const option = options[i].trim();

        if (option == '') continue;

        if (regex.test(option) === false) {
            diff.push(`  ${i}: ${options[i]}`);
        }
    }

    return diff;
}
