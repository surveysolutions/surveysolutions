import { get, post, commandCall } from '../services/apiService';
import emitter from './emitter';
import { toLocalDateTime } from '../../../services/utilityService';

export function updateQuestionnaire(questionnaireId, questionnaire) {
    var command = {
        title: questionnaire.title,
        variable: questionnaire.variable,
        questionnaireId: questionnaireId,
        hideifDisabled: questionnaire.hideifDisabled,
        isPublic: questionnaire.isPublic,
        defaultLanguageName: questionnaire.defaultLanguageName
    };

    return commandCall('UpdateQuestionnaire', command).then(async response => {
        emitter.emit('questionnaireUpdated', {
            title: questionnaire.title,
            variable: questionnaire.variable,
            questionnaireId: questionnaireId,
            hideifDisabled: questionnaire.hideifDisabled,
            isPublic: questionnaire.isPublic,
            defaultLanguageName: questionnaire.defaultLanguageName
        });
    });
}

export async function regenerateAnonymousQuestionnaireLink(questionnaireId) {
    var regenerateUrl =
        '/questionnaire/regenerateAnonymousQuestionnaireLink/' +
        questionnaireId;

    return await post(regenerateUrl).then(response => {
        var data = response;
        emitter.emit('anonymousQuestionnaireSettingsUpdated', {
            isAnonymouslyShared: data.isActive,
            anonymousQuestionnaireId: data.isActive
                ? data.anonymousQuestionnaireId
                : null,
            anonymousQuestionnaireShareDate: toLocalDateTime(
                data.anonymouslySharedAtUtc
            )
        });
    });
}

export async function updateAnonymousQuestionnaireSettings(questionnaireId, isActive) {
  var regenerateUrl =
      '/questionnaire/updateAnonymousQuestionnaireSettings/' + questionnaireId;

  return await post(regenerateUrl, { isActive: isActive }).then(response => {
      var data = response;
      emitter.emit('anonymousQuestionnaireSettingsUpdated', {
        isAnonymouslyShared: data.isActive,
        anonymousQuestionnaireId: data.isActive
            ? data.anonymousQuestionnaireId
            : null,
        anonymousQuestionnaireShareDate: toLocalDateTime(
            data.anonymouslySharedAtUtc
        )
    });
  });
}


updateAnonymousQuestionnaireSettings = function(questionnaireId, isActive) {
  var baseUrl = '../../questionnaire/updateAnonymousQuestionnaireSettings/' + questionnaireId;
  return $http.post(baseUrl, { isActive: isActive }, { headers: { 'X-CSRF-TOKEN': getCsrfCookie()} });
};
