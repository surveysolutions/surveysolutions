import { get, commandCall } from '../services/apiService';
import emitter from './emitter';

export async function findAll(questionnaireId, searchForm) {
    const data = await get('/api/findReplace/findAll', {
        id: questionnaireId,
        searchFor: searchForm.searchFor,
        matchCase: searchForm.matchCase,
        matchWholeWord: searchForm.matchWholeWord,
        useRegex: searchForm.useRegex
    });
    return data;
}

export function replaceAll(questionnaireId, searchForm) {
    var command = {
        questionnaireId: questionnaireId,
        searchFor: searchForm.searchFor,
        replaceWith: searchForm.replaceWith,
        matchCase: searchForm.matchCase,
        matchWholeWord: searchForm.matchWholeWord,
        useRegex: searchForm.useRegex
    };

    return commandCall('ReplaceTexts', command).then(async response => {
        emitter.emit('textsReplaced', {});
    });
}
