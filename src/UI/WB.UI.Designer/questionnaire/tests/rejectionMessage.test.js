import assert from 'node:assert/strict';
import { mock, test } from 'node:test';
import { getRejectionMessage } from '../src/helpers/rejectionMessage.js';

test('preserves Error messages and HTTP response message precedence', () => {
    assert.equal(getRejectionMessage(new Error('Failure')), 'Failure');
    assert.equal(getRejectionMessage({
        message: 'HTTP error',
        body: { message: 'Server error' }
    }), 'Server error');
});

test('preserves string and primitive rejection reasons', () => {
    assert.equal(getRejectionMessage('Timeout'), 'Timeout');
    assert.equal(getRejectionMessage(0), '0');
    assert.equal(getRejectionMessage(false), 'false');
    assert.equal(getRejectionMessage(123n), '"123"');
    assert.equal(getRejectionMessage(Symbol('failure')), '"Symbol(failure)"');
});

test('provides explicit messages for missing and empty reasons', () => {
    assert.equal(getRejectionMessage(undefined), 'Unhandled promise rejection (undefined)');
    assert.equal(getRejectionMessage(null), 'Unhandled promise rejection (null)');
    assert.equal(getRejectionMessage(''), 'Unhandled promise rejection (empty string)');
    assert.equal(getRejectionMessage('   '), '"   "');
    assert.equal(getRejectionMessage(new Error('')), '{}');
});

test('serializes object reasons to a string compatible with the report model', () => {
    const reason = { code: 'captcha-error', details: { attempt: 2 } };
    const report = JSON.parse(JSON.stringify({ message: getRejectionMessage(reason) }));
    assert.equal(typeof report.message, 'string');
    assert.deepEqual(JSON.parse(report.message), reason);
});

test('handles circular references and nested BigInts', () => {
    const reason = { code: 'failure', attempt: 1n };
    reason.self = reason;
    assert.deepEqual(JSON.parse(getRejectionMessage(reason)), {
        code: 'failure',
        attempt: '1',
        self: '[Circular or repeated reference]'
    });
});

test('redacts credential fields in object reasons', () => {
    const reason = {
        code: 'failure',
        Input: { Password: 'private', ConfirmPassword: 'private' },
        access_token: 'private',
        headers: { Authorization: 'private', Cookie: 'private' },
        secret: 'private'
    };
    const result = JSON.parse(getRejectionMessage(reason));
    assert.equal(result.code, 'failure');
    assert.equal(result.Input.Password, '[REDACTED]');
    assert.equal(result.Input.ConfirmPassword, '[REDACTED]');
    assert.equal(result.access_token, '[REDACTED]');
    assert.equal(result.headers.Authorization, '[REDACTED]');
    assert.equal(result.headers.Cookie, '[REDACTED]');
    assert.equal(result.secret, '[REDACTED]');
});

test('logs serialization failures without losing the rejection report', () => {
    const serializationError = new Error('Cannot serialize');
    const log = mock.method(console, 'error', () => {});
    try {
        const reason = { toJSON() { throw serializationError; } };
        assert.equal(getRejectionMessage(reason),
            'Unhandled promise rejection (reason could not be serialized)');
        assert.equal(log.mock.calls.length, 1);
        assert.equal(log.mock.calls[0].arguments[1], serializationError);
    } finally {
        log.mock.restore();
    }
});
