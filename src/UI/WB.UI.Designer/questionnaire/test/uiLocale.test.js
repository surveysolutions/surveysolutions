import assert from 'node:assert/strict';
import test from 'node:test';
import {
    getTextDirection,
    getUiDirection,
    isRtlLocale,
    mirrorPlacement,
    normalizeUiLocale,
} from '../src/plugins/uiLocale.js';

test('normalizes UI locales and identifies RTL cultures', () => {
    assert.equal(normalizeUiLocale('ar_EG'), 'ar');
    assert.equal(getUiDirection('ar'), 'rtl');
    assert.equal(getUiDirection('en-US'), 'ltr');
    assert.equal(isRtlLocale('fr'), false);
});

test('mirrors only directional overlay placement', () => {
    assert.equal(mirrorPlacement('left', 'rtl'), 'right');
    assert.equal(mirrorPlacement('right', 'rtl'), 'left');
    assert.equal(mirrorPlacement('top', 'rtl'), 'top');
    assert.equal(mirrorPlacement('left', 'ltr'), 'left');
});

test('derives content direction from its first strong character', () => {
    assert.equal(getTextDirection('استبيان'), 'rtl');
    assert.equal(getTextDirection('English title'), 'ltr');
    assert.equal(getTextDirection('12345'), 'ltr');
    assert.equal(getTextDirection('https://example.org/مسار'), 'ltr');
    assert.equal(getTextDirection('مرحبا %variable%'), 'rtl');
    assert.equal(getTextDirection('%variable% مرحبا'), 'ltr');
    assert.equal(getTextDirection(''), 'ltr');
});
