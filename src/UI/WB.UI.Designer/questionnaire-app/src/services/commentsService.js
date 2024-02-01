import { get, patch, del, post, commandCall } from './apiService';
import emitter from './emitter';
import _ from 'lodash';
import moment from 'moment';
import { newGuid } from '../helpers/guid';

export async function getComments(questionnaireId, entityId) {
    return await get(
        'questionnaire/' + questionnaireId + '/entity/' + entityId + '/comments'
    );
}

export function resolveComment(questionnaireId, commentId, entityId) {
    return patch(
        'questionnaire/' + questionnaireId + '/comment/resolve/' + commentId
    ).then(response => {
        emitter.emit('commentResolved', {
            id: commentId,
            resolveDate: new Date(),
            entityId: entityId
        });
    });
}

export function deleteComment(questionnaireId, commentId, entityId) {
    return del(
        'questionnaire/' + questionnaireId + '/comment/' + commentId
    ).then(response => {
        emitter.emit('commentDeleted', {
            id: commentId,
            entityId: entityId
        });
    });
}

export async function postComment(
    questionnaireId,
    comment,
    entityId,
    userName,
    userEmail,
    chapterId,
    title,
    variable,
    type
) {
    const id = newGuid();

    return await post(
        'questionnaire/' + questionnaireId + '/entity/addComment',
        {
            comment: comment,
            entityId: entityId,
            id: id,
            questionnaireId: questionnaireId
        }
    ).then(response => {
        emitter.emit('commentAdded', {
            entityId: entityId,
            comment: comment,
            date: moment(new Date()).format('LLL'),
            id: id,
            userName: userName,
            userEmail: userEmail,
            title,
            variable,
            type,
            chapterId
        });
    });
}

export async function getCommentThreads(questionnaireId) {
    const data = await get(
        'questionnaire/' + questionnaireId + '/commentThreads'
    );

    return data;
}
