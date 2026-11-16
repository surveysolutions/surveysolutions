export function getRejectionMessage(reason) {
    const message = reason?.body?.message ?? reason?.message;
    if (typeof message === 'string' && message.trim()) return message;
    if (typeof reason === 'string' && reason.trim()) return reason;
    if (reason == null || reason === '') {
        return `Unhandled promise rejection (${reason === '' ? 'empty string' : String(reason)})`;
    }

    const seen = new WeakSet();
    try {
        return JSON.stringify(reason, (key, value) => {
            if (/password|token|secret|authorization|cookie/i.test(key)) {
                return '[REDACTED]';
            }
            if (typeof value === 'object' && value !== null) {
                if (seen.has(value)) return '[Circular or repeated reference]';
                seen.add(value);
            }
            if (['bigint', 'symbol', 'function', 'undefined'].includes(typeof value)) {
                return String(value);
            }
            return value;
        }) ?? 'Unhandled promise rejection (no serializable reason)';
    } catch (error) {
        console.error('Error serializing promise rejection reason:', error);
        return 'Unhandled promise rejection (reason could not be serialized)';
    }
}
