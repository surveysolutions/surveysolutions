import fs from 'node:fs';
import path from 'node:path';

const root = path.resolve(import.meta.dirname, '../..');
const allowedFixedPhysical = '/* direction: fixed */';
const physicalProperty = /(^\s*|\s)(?:left|right)\s*:|(?:margin|padding)-(?:left|right)\s*:|border-(?:left|right)(?:-[\w-]+)?\s*:|text-align\s*:\s*(?:left|right)\b|float\s*:\s*(?:left|right)\b/;
const directionalClass = /\b(?:pull-left|pull-right|dropdown-menu-left|dropdown-menu-right)\b/;
const files = [];

function walk(directory) {
    for (const entry of fs.readdirSync(directory, { withFileTypes: true })) {
        const file = path.join(directory, entry.name);
        if (entry.isDirectory()) {
            if (entry.name === 'external' || entry.name === 'Pdf' || entry.name === 'Resources') continue;
            walk(file);
        } else if (/\.(?:less|css|vue|cshtml)$/.test(entry.name) && !entry.name.endsWith('.min.css') && entry.name !== 'pdf.css' && entry.name !== 'pdf.less') {
            files.push(file);
        }
    }
}

walk(path.join(root, 'questionnaire/content'));
walk(path.join(root, 'questionnaire/src'));
walk(path.join(root, 'Content'));
walk(path.join(root, 'Views'));
walk(path.join(root, 'Areas'));

const violations = [];
for (const file of files) {
    const source = fs.readFileSync(file, 'utf8');
    const sourceLines = source.split(/\r?\n/);
    const cssSource = file.endsWith('.vue')
        ? [...source.matchAll(/<style\b[^>]*>([\s\S]*?)<\/style>/g)].map(match => match[1]).join('\n')
        : source;
    const lines = cssSource.split(/\r?\n/);

    for (let index = 0; index < lines.length; index++) {
        const line = lines[index];
        const isAllowedFixed = lines.slice(Math.max(0, index - 2), index + 1).some(item => item.includes(allowedFixedPhysical));
        if (physicalProperty.test(line) && !isAllowedFixed) {
            violations.push(`${path.relative(root, file)}:${index + 1}: physical CSS property`);
        }
    }

    for (let index = 0; index < sourceLines.length; index++) {
        if (/\.(?:vue|cshtml)$/.test(file) && directionalClass.test(sourceLines[index])) {
            violations.push(`${path.relative(root, file)}:${index + 1}: directional utility class`);
        }
    }
}

if (violations.length > 0) {
    console.error(violations.join('\n'));
    process.exitCode = 1;
}
