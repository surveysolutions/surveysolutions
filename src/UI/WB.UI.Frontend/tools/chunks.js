const fs = require("fs")
const _ = require('lodash')

module.exports = function getChunkFiles(dist, pageName) {
    var statsJson = fs.readFileSync(dist + "/stats.json")
    var stats = JSON.parse(statsJson)
    const filesToCopy = []
    const queue = [pageName]

    while(id = queue.pop()) {
        let chunk = _.find(stats.chunks, { id })

        chunk.files.forEach(element => {
            filesToCopy.push(element)
        });

        chunk.children.forEach(child => {
            queue.push(child)
        });
    }

    return _.uniq(filesToCopy)
}
