export function isInteger(value) {
    return value == null || /^([-+]?\d+)$/.test(value);
}

export function isNumber(value) {
    return value == null || /^([-+]?\d+)$/.test(value);
}
