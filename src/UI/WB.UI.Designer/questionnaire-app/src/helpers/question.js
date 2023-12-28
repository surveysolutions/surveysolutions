import { i18n } from '../plugins/localization';

export const answerTypeClass = {
    YesNo: 'icon-singleoption',
    DropDownList: 'icon-singleoption',
    MultyOption: 'icon-multyoption',
    Numeric: 'icon-numeric',
    DateTime: 'icon-datetime',
    GpsCoordinates: 'icon-gpscoordinates',
    AutoPropagate: 'icon-textedit',
    TextList: 'icon-textlist',
    QRBarcode: 'icon-qrbarcode',
    Text: 'icon-text',
    SingleOption: 'icon-singleoption',
    Multimedia: 'icon-photo',
    Area: 'icon-area',
    Audio: 'icon-audio'
};

export const categoricalMultiKinds = [
    { value: 1, text: i18n.t('QuestionnaireEditor.QuestionCheckboxes') },
    { value: 2, text: i18n.t('QuestionnaireEditor.QuestionYesNoMode') },
    { value: 3, text: i18n.t('QuestionnaireEditor.QuestionComboBox') }
];

export const geometryInputModeOptions = [
    {
        value: 'Manual',
        text: i18n.t('QuestionnaireEditor.GeographyInputModeManual')
    },
    {
        value: 'Automatic',
        text: i18n.t('QuestionnaireEditor.GeographyInputModeAutomatic')
    },
    {
        value: 'Semiautomatic',
        text: i18n.t('QuestionnaireEditor.GeographyInputModeSemiautomatic')
    }
];
