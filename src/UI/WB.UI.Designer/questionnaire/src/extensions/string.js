export function format() {
    var args = arguments;
    var sprintfRegex = /\{(\d+)\}/g;

    var sprintf = function(match, number) {
        return number in args ? args[number] : match;
    };

    return this.replace(sprintfRegex, sprintf);
}

if (!String.prototype.format) {
    String.prototype.format = format;
}
