import { commandCall } from './apiService';
import emitter from './emitter';

export function updateMetadata(questionnaireId, metadata) {
    var command = {
        questionnaireId: questionnaireId,
        title: metadata.title,
        metadata: {
            subTitle: metadata.subTitle,
            studyType: metadata.studyType,
            version: metadata.version,
            versionNotes: metadata.versionNotes,
            kindOfData: metadata.kindOfData,
            country: metadata.country,
            year: metadata.year,
            language: metadata.language,
            coverage: metadata.coverage,
            universe: metadata.universe,
            unitOfAnalysis: metadata.unitOfAnalysis,
            primaryInvestigator: metadata.primaryInvestigator,
            funding: metadata.funding,
            consultant: metadata.consultant,
            modeOfDataCollection: metadata.modeOfDataCollection,
            notes: metadata.notes,
            keywords: metadata.keywords,
            agreeToMakeThisQuestionnairePublic:
                metadata.agreeToMakeThisQuestionnairePublic
        }
    };

    return commandCall('UpdateMetadata', command).then(response => {
        emitter.emit('metadataUpdated', {
            metadata: metadata
        });
    });
}
