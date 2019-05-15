const through2 = require('through2');
const vinylFile = require('vinyl-file');
const crypto = require('crypto');
const path = require('path');
const fs = require('fs');

const calculateHash = data => {
    const hasher = crypto.createHash('sha1');
    const hash = hasher.update(data).digest('hex');
    return hash;
};

const dest_changed = function(dest) {
    return through2.obj((file, _, done) => {
        const incomingHash = calculateHash(file.contents);

        let destinationFile = typeof dest === 'function' ? dest(file) : dest;

        if (fs.statSync(destinationFile).isDirectory()) {
            destinationFile = path.join(destinationFile, file.basename);
        }

        vinylFile.read(destinationFile).then(f => {
            const destinationHash = calculateHash(f.contents);
            const sameHash = incomingHash === destinationHash;
            if (!sameHash) {
                done(null, file);
            } else {
                done(null);
            }
        });
    });
};

module.exports = dest_changed;
