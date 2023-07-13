<template>
    <HqLayout :mainClass="'interview-setup'" :title="$t('WebInterviewSettings.WebInterviewSetupFor_Title')">
        <div slot="headers">
            <ol class="breadcrumb">
                <li>
                    <a :href="this.$config.model.surveySetupUrl">{{ $t('MainMenu.SurveySetup') }}</a>
                </li>
            </ol>
            <h1>
                {{ this.$t('WebInterviewSettings.WebInterviewSetupFor_Title') }}
                <b>
                    {{ $t('Pages.QuestionnaireNameFormat', {
                        name: this.$config.model.questionnaireTitle,
                        version: this.$config.model.questionnaireVersion
                    }) }}
                </b>
            </h1>
        </div>

        <div class="row">
            <div class="col-md-12">
                <h3>{{ $t('WebInterviewSettings.CustomizeDisplayedText') }}</h3>
                <div class="welcome-page">
                    <ul class="nav nav-tabs" role="tablist" id="start-screen-example">
                        <li role="presentation" class="active">
                            <a href="#welcome" @click="setPageActive('welcomeTextTitle', 'welcomeTextDescription')"
                                aria-controls="welcome" role="tab" data-toggle="tab">{{
                                    $t('WebInterviewSettings.WelcomePage') }}</a>
                        </li>
                        <li role="presentation">
                            <a href="#resume" @click="setPageActive('resumeWelcome', 'resumeInvitation')"
                                aria-controls="resume" role="tab" data-toggle="tab">{{ $t('WebInterviewSettings.ResumePage')
                                }}</a>
                        </li>
                        <li role="presentation">
                            <a href="#complete" @click="setPageActive('completeNoteToSupervisor')" aria-controls="complete"
                                role="tab" data-toggle="tab">{{ $t('WebInterviewSettings.CompletePage') }}</a>
                        </li>
                        <li role="presentation">
                            <a href="#finish" @click="setPageActive('webSurveyHeader', 'finishInterview')"
                                aria-controls="finish" role="tab" data-toggle="tab">{{ $t('WebInterviewSettings.FinishPage')
                                }}</a>
                        </li>
                        <li role="presentation">
                            <a href="#link" @click="setPageActive('linkWelcome', 'linkInvitation')" aria-controls="link"
                                role="tab" data-toggle="tab">{{ $t('WebInterviewSettings.LinkToInterviewPage') }}</a>
                        </li>
                    </ul>
                    <div class="tab-content">
                        <Welcome :webInterviewPageMessages="webInterviewPageMessages" :hasLogo="hasLogo" :logoUrl="logoUrl"
                            :questionnaireId="questionnaireId" :questionnaireTitle="questionnaireTitle" />
                        <Resume :webInterviewPageMessages="webInterviewPageMessages" :hasLogo="hasLogo" :logoUrl="logoUrl"
                            :questionnaireId="questionnaireId" :questionnaireTitle="questionnaireTitle" />
                        <Complete :webInterviewPageMessages="webInterviewPageMessages" :hasLogo="hasLogo" :logoUrl="logoUrl"
                            :questionnaireId="questionnaireId" :questionnaireTitle="questionnaireTitle" />
                        <Finish :webInterviewPageMessages="webInterviewPageMessages" :hasLogo="hasLogo" :logoUrl="logoUrl"
                            :questionnaireId="questionnaireId" :questionnaireTitle="questionnaireTitle" />
                        <LinkInterview :webInterviewPageMessages="webInterviewPageMessages" :hasLogo="hasLogo"
                            :logoUrl="logoUrl" :questionnaireId="questionnaireId"
                            :questionnaireTitle="questionnaireTitle" />
                    </div>
                </div>
            </div>
        </div>
        <hr />
        <div class="row mb-05">
            <div class="col-md-12">
                <h3>{{ $t('WebInterviewSettings.CustomizeEmailsText') }}</h3>
                <div>
                    <ul class="nav nav-tabs" role="tablist">
                        <li v-for="emailTemplate in emailTemplates" :key="emailTemplate.value"
                            :class="{ active: emailTemplate.isActive }" role="presentation">
                            <a href="javascript:void(0);" role="tab" data-toggle="tab"
                                @click.stop.prevent="setActive(emailTemplate)">{{ emailTemplate.buttonTitle }}</a>
                        </li>
                    </ul>

                    <div class="tab-content">
                        <div v-for="emailTemplate in emailTemplates" :key="emailTemplate.type"
                            :class="{ active: emailTemplate.isActive }" role="tabpanel" class="tab-pane email-section">
                            <form v-on:submit.prevent="dummy" :data-vv-scope="'emailTemplateData' + emailTemplate.value">
                                <div class="email-block d-flex mb-30">
                                    <div class="costomization-block email-block-unit">
                                        <div class="">
                                            <div class="row-element mb-30">
                                                <p>{{ $t('WebInterviewSettings.EmailTemplateDescription') }}</p>
                                            </div>
                                            <div class="row-element">
                                                <div class="h5">
                                                    {{ $t('WebInterviewSettings.EmailSubject') }}
                                                </div>
                                                <div class="form-group mb-30"
                                                    :class="{ 'has-error': errors.has('emailTemplateData' + emailTemplate.value + '.subject') }">
                                                    <div class="field" :class="{ 'answered': emailTemplate.subject }">
                                                        <input type="text" v-model="emailTemplate.subject"
                                                            data-vv-as="Please enter the subject" v-validate="'required'"
                                                            data-vv-name="subject" maxlength="200"
                                                            class="form-control with-clear-btn"
                                                            placeholder="Please enter the subject">
                                                        <button type="button" @click="clearField(emailTemplate, 'subject')"
                                                            class="btn btn-link btn-clear">
                                                            <span></span>
                                                        </button>
                                                        <span class="help-block"
                                                            v-if="errors.first('emailTemplateData' + emailTemplate.value + '.subject')">{{
                                                                $t('WebInterviewSettings.FieldRequired') }}</span>
                                                    </div>
                                                </div>
                                            </div>
                                            <div class="row-element mb-30"
                                                v-if="isMessageSupportedInterviewData(emailTemplate)">
                                                <p>{{ $t('WebInterviewSettings.InterviewDataInsertInTextDescription') }}</p>
                                                <p>{{ $t('WebInterviewSettings.InterviewDataInsertBarcodeDescription') }}
                                                </p>
                                                <p>{{ $t('WebInterviewSettings.InterviewDataInsertQrCodeDescription') }}</p>
                                            </div>
                                            <div class="row-element">
                                                <div class="h5">
                                                    {{ $t('WebInterviewSettings.MainText') }}
                                                </div>
                                                <div class="form-group mb-30"
                                                    :class="{ 'has-error': errors.has('emailTemplateData' + emailTemplate.value + '.message') }">
                                                    <div class="field" :class="{ 'answered': emailTemplate.message }">
                                                        <textarea-autosize v-model="emailTemplate.message"
                                                            data-vv-as="Please enter the main text" v-validate="'required'"
                                                            data-vv-name="message" :ref="'message' + emailTemplate.value"
                                                            maxlength="3000" :min-height="79"
                                                            class="form-control js-elasticArea"
                                                            placeholder="Please enter the main text">
                                                        </textarea-autosize>
                                                        <button type="button" @click="emailTemplate.message = null"
                                                            class="btn btn-link btn-clear">
                                                            <span></span>
                                                        </button>
                                                        <span class="help-block"
                                                            v-if="errors.first('emailTemplateData' + emailTemplate.value + '.message')">{{
                                                                $t('WebInterviewSettings.FieldRequired') }}</span>
                                                    </div>
                                                </div>
                                            </div>
                                            <div class="row-element" v-if="isPasswordSupported(emailTemplate)">
                                                <div class="h5 mb-0">
                                                    {{ $t('WebInterviewSettings.DescriptionForPassword') }}
                                                </div>
                                                <div class="gray-text mb-1">
                                                    {{ $t('WebInterviewSettings.ShownPasswordIsRequired') }}
                                                </div>
                                                <div class="form-group mb-30"
                                                    :class="{ 'has-error': errors.has('emailTemplateData' + emailTemplate.value + '.passwordDescription') }">
                                                    <div class="field"
                                                        :class="{ 'answered': emailTemplate.passwordDescription }">
                                                        <input type="text" v-model="emailTemplate.passwordDescription"
                                                            data-vv-name="passwordDescription"
                                                            data-vv-as="Please enter password description"
                                                            v-validate="'required'" maxlength="500"
                                                            class="form-control with-clear-btn"
                                                            placeholder="Please enter password description">
                                                        <button type="button"
                                                            @click="clearField(emailTemplate, 'passwordDescription')"
                                                            class="btn btn-link btn-clear">
                                                            <span></span>
                                                        </button>
                                                        <span class="help-block"
                                                            v-if="errors.first('emailTemplateData' + emailTemplate.value + '.passwordDescription')">{{
                                                                $t('WebInterviewSettings.FieldRequired') }}</span>
                                                    </div>
                                                </div>
                                            </div>
                                            <div class="row-element" v-if="isButtonSupported(emailTemplate)">
                                                <div class="h5">
                                                    {{ $t('WebInterviewSettings.StartInterviewButton') }}
                                                </div>
                                                <div class="form-group mb-30"
                                                    :class="{ 'has-error': errors.has('emailTemplateData' + emailTemplate.value + '.linkText') }">
                                                    <div class="field" :class="{ 'answered': emailTemplate.linkText }">
                                                        <span class="wrapper-dynamic">
                                                            <input type="text" v-model="emailTemplate.linkText"
                                                                v-validate="'required'" data-vv-name="linkText"
                                                                maxlength="200"
                                                                class="form-control with-clear-btn width-dynamic"
                                                                placeholder="Please enter the text" />
                                                            <button type="button"
                                                                @click="clearField(emailTemplate, 'linkText')"
                                                                class="btn btn-link btn-clear">
                                                                <span></span>
                                                            </button>
                                                            <span class="help-block"
                                                                v-if="errors.first('emailTemplateData' + emailTemplate.value + '.linkText')">{{
                                                                    $t('WebInterviewSettings.FieldRequired') }}</span>
                                                        </span>
                                                    </div>
                                                </div>
                                            </div>
                                            <div class="">
                                                <button type="submit"
                                                    :disabled="!isDirty('$emailTemplateData' + emailTemplate.value)"
                                                    @click="saveEmailTemplate(emailTemplate)"
                                                    class="btn btn-md btn-success">
                                                    {{ $t('WebInterviewSettings.Save') }}
                                                </button>
                                                <button type="button"
                                                    :disabled="!isDirty('$emailTemplateData' + emailTemplate.value)"
                                                    @click="cancelEditEmailTemplate(emailTemplate)"
                                                    class="btn btn-md btn-link">
                                                    {{ $t('WebInterviewSettings.Cancel') }}
                                                </button>
                                            </div>
                                        </div>
                                    </div>

                                    <div class="preview email-block-unit">
                                        <div class="browser-mockup">
                                            <div class="email-example">
                                                <table class="em-table email-example-table" align="center" border="0"
                                                    cellspacing="0" cellpadding="0">
                                                    <tr>
                                                        <td
                                                            style="border: 6px solid #E5E5E5;  padding: 50px 55px 115px; box-sizing: border-box;">
                                                            <table border="0" width="100%" cellpadding="0" cellspacing="0">
                                                                <tr>
                                                                    <td style="padding-bottom: 80px;">
                                                                        <div v-if="!hasLogo" class="default-icon">
                                                                            <svg width="178" height="83"
                                                                                viewBox="0 0 178 83" fill="none"
                                                                                xmlns="http://www.w3.org/2000/svg">
                                                                                <path fill-rule="evenodd"
                                                                                    clip-rule="evenodd"
                                                                                    d="M14.0173 53.3122C14.2271 52.8087 14.437 52.3261 14.6258 51.8226C15.9478 48.5915 15.9898 48.4656 15.9898 44.5632V28.9745H12.0238V44.6052C12.0238 48.4866 12.0868 48.6335 13.3878 51.8016C13.5766 52.2841 13.8075 52.7877 14.0173 53.3122ZM15.8849 59.3966C16.7033 57.3825 17.5216 55.3683 18.34 53.3542C19.9558 49.3888 20.0187 49.221 20.0187 44.5632V26.9603C20.0187 25.5966 19.62 24.9462 18.0043 24.9462H12.0238V20.813H23.9847V44.4793C23.9847 50.1021 23.8378 50.4588 22.0122 54.8858C19.3263 61.4318 16.6613 67.9988 13.9753 74.5448C11.2894 67.9568 8.62442 61.3479 5.91749 54.7809C4.21778 50.6686 4.00794 50.1441 4.00794 44.7101V28.9745H7.97392V44.6052C7.97392 49.2419 8.05785 49.4308 9.65264 53.3332L12.1288 59.4176C13.1779 61.9143 14.9196 61.7884 15.8849 59.3966ZM2.01446 24.9252H7.9949V18.7778C7.9949 17.2252 8.62442 16.7637 10.0094 16.7637H26.0202C27.6359 16.7637 28.0346 17.4141 28.0346 18.7778V44.4793C28.0346 50.8575 27.8667 51.2561 25.7474 56.4174C22.4529 64.495 19.1584 72.5726 15.8639 80.6292C14.5839 83.7973 13.3878 83.7553 12.1288 80.6292C8.83426 72.5096 5.53977 64.3901 2.2243 56.2915C0.251808 51.5079 0 50.8994 0 44.7101V26.9603C0 25.8274 0.293776 24.9252 2.01446 24.9252Z"
                                                                                    fill="#303030" />
                                                                                <path fill-rule="evenodd"
                                                                                    clip-rule="evenodd"
                                                                                    d="M4.02893 19.8059C4.02893 20.9178 3.12662 21.0018 2.01446 21.0018C0.902312 21.0018 0 20.9178 0 19.8059V10.4484C0 9.08468 0.377712 8.43428 2.01446 8.43428H24.0057V4.02831H1.19609C0.083936 4.02831 0 3.12614 0 2.01416C0 0.902174 0.083936 0 1.19609 0H26.0202C27.594 0 28.0346 0.503539 28.0346 2.01416V10.4484C28.0346 11.7073 27.6359 12.4626 26.0202 12.4626H4.02893V19.8059Z"
                                                                                    fill="#303030" />
                                                                                <path
                                                                                    d="M58.2306 48.8433C58.2306 51.4659 60.266 52.4729 62.5952 52.4729C64.1061 52.4729 66.4353 52.0323 66.4353 50.0182C66.4353 47.8781 63.4556 47.5634 60.5808 46.7662C57.664 46.0528 54.7053 44.8149 54.7053 41.1643C54.7053 37.115 58.5034 35.2057 62.0706 35.2057C66.1835 35.2057 69.9606 36.9891 69.9606 41.5419H65.7638C65.6169 39.1921 63.9802 38.5627 61.8818 38.5627C60.4968 38.5627 58.9021 39.1501 58.9021 40.7866C58.9021 42.2973 59.8463 42.528 64.7776 43.7659C66.2465 44.1016 70.6321 45.0667 70.6321 49.4937C70.6321 53.1024 67.8203 55.8089 62.4484 55.8089C58.0837 55.8089 53.9918 53.6688 54.0757 48.8013H58.2306V48.8433Z"
                                                                                    fill="#303030" />
                                                                                <path
                                                                                    d="M79.8441 40.7237C84.3347 40.7237 87.2305 43.703 87.2305 48.2558C87.2305 52.7667 84.3347 55.7459 79.8441 55.7459C75.3745 55.7459 72.4788 52.7667 72.4788 48.2558C72.4788 43.703 75.3745 40.7237 79.8441 40.7237ZM79.8441 52.8086C82.5091 52.8086 83.3275 50.5427 83.3275 48.2558C83.3275 45.9689 82.5301 43.682 79.8441 43.682C77.2002 43.682 76.4028 45.9689 76.4028 48.2558C76.4028 50.5427 77.2002 52.8086 79.8441 52.8086Z"
                                                                                    fill="#303030" />
                                                                                <path
                                                                                    d="M89.9584 35.6883H93.8824V55.3683H89.9584V35.6883Z"
                                                                                    fill="#303030" />
                                                                                <path
                                                                                    d="M110.481 55.3683H106.767V53.3751H106.683C105.696 54.9906 103.976 55.7459 102.318 55.7459C98.1631 55.7459 97.1139 53.3961 97.1139 49.8713V41.1223H101.038V49.1789C101.038 51.5288 101.709 52.6827 103.556 52.6827C105.675 52.6827 106.599 51.5078 106.599 48.6125V41.1223H110.523V55.3683H110.481Z"
                                                                                    fill="#303030" />
                                                                                <path
                                                                                    d="M118.622 41.1223H121.497V43.7449H118.622V50.7944C118.622 52.1162 118.958 52.4519 120.28 52.4519C120.721 52.4519 121.141 52.4309 121.497 52.347V55.4102C120.826 55.5151 119.986 55.5571 119.21 55.5571C116.776 55.5571 114.719 55.0326 114.719 52.1582V43.7449H112.348V41.1223H114.719V36.8422H118.643V41.1223H118.622Z"
                                                                                    fill="#303030" />
                                                                                <path
                                                                                    d="M127.835 38.9193H123.911V35.6883H127.835V38.9193ZM123.932 41.1223H127.856V55.3683H123.932V41.1223Z"
                                                                                    fill="#303030" />
                                                                                <path
                                                                                    d="M137.928 40.7237C142.418 40.7237 145.314 43.703 145.314 48.2558C145.314 52.7667 142.418 55.7459 137.928 55.7459C133.458 55.7459 130.562 52.7667 130.562 48.2558C130.562 43.703 133.458 40.7237 137.928 40.7237ZM137.928 52.8086C140.593 52.8086 141.411 50.5427 141.411 48.2558C141.411 45.9689 140.614 43.682 137.928 43.682C135.284 43.682 134.486 45.9689 134.486 48.2558C134.486 50.5427 135.284 52.8086 137.928 52.8086Z"
                                                                                    fill="#303030" />
                                                                                <path
                                                                                    d="M147.979 41.1223H151.693V43.1155H151.777C152.763 41.479 154.484 40.7447 156.142 40.7447C160.297 40.7447 161.346 43.0945 161.346 46.6193V55.3893H157.422V47.3327C157.422 44.9828 156.75 43.8289 154.904 43.8289C152.784 43.8289 151.861 45.0038 151.861 47.8991V55.3683H147.937V41.1223H147.979Z"
                                                                                    fill="#303030" />
                                                                                <path
                                                                                    d="M167.515 50.7315C167.515 52.41 169.005 53.1233 170.516 53.1233C171.628 53.1233 173.076 52.6827 173.076 51.298C173.076 50.1231 171.418 49.6615 168.627 49.095C166.34 48.5705 164.116 47.8362 164.116 45.2975C164.116 41.6678 167.264 40.7237 170.327 40.7237C173.412 40.7237 176.308 41.7727 176.622 45.2765H172.908C172.803 43.7659 171.649 43.3463 170.243 43.3463C169.362 43.3463 168.061 43.4932 168.061 44.6681C168.061 46.0738 170.285 46.2626 172.51 46.7871C174.797 47.3117 177.021 48.1299 177.021 50.7945C177.021 54.55 173.748 55.7459 170.516 55.7459C167.201 55.7459 163.948 54.5081 163.822 50.7315H167.515Z"
                                                                                    fill="#303030" />
                                                                                <path
                                                                                    d="M58.6712 19.5331C58.6712 22.1557 60.7067 23.1628 63.0359 23.1628C64.5468 23.1628 66.876 22.7222 66.876 20.7081C66.876 18.568 63.8962 18.2533 61.0214 17.456C58.1047 16.7427 55.1459 15.5048 55.1459 11.8542C55.1459 7.80488 58.944 5.89563 62.5113 5.89563C66.6242 5.89563 70.4013 7.679 70.4013 12.2318H66.2045C66.0576 9.88198 64.4209 9.25256 62.3225 9.25256C60.9375 9.25256 59.3427 9.84002 59.3427 11.4765C59.3427 12.9871 60.287 13.2179 65.2182 14.4558C66.6871 14.7915 71.0728 15.7566 71.0728 20.1835C71.0728 23.7922 68.2609 26.4988 62.889 26.4988C58.5243 26.4988 54.4325 24.3587 54.5164 19.4912H58.6712V19.5331Z"
                                                                                    fill="#303030" />
                                                                                <path
                                                                                    d="M86.8738 26.0582H83.1596V24.065H83.0757C82.0894 25.6805 80.3687 26.4358 78.711 26.4358C74.5562 26.4358 73.507 24.086 73.507 20.5612V11.8122H77.431V19.8688C77.431 22.2187 78.1025 23.3726 79.949 23.3726C82.0684 23.3726 82.9917 22.1977 82.9917 19.3023V11.8122H86.9157V26.0582H86.8738Z"
                                                                                    fill="#303030" />
                                                                                <path
                                                                                    d="M90.1052 11.8122H93.8194V14.4558H93.8824C94.6168 12.6724 96.4634 11.4346 98.4359 11.4346C98.7297 11.4346 99.0234 11.4975 99.2962 11.5814V15.2111C98.9395 15.1272 98.3519 15.0642 97.8903 15.0642C94.9945 15.0642 94.0292 17.0994 94.0292 19.6381V26.0582H90.1052V11.8122Z"
                                                                                    fill="#303030" />
                                                                                <path
                                                                                    d="M109.62 26.0581H105.256L100.366 11.8122H104.479L107.48 21.5473H107.543L110.544 11.8122H114.426L109.62 26.0581Z"
                                                                                    fill="#303030" />
                                                                                <path
                                                                                    d="M118.979 19.8898C119.084 22.4075 120.28 23.4985 122.483 23.4985C124.057 23.4985 125.316 22.5334 125.568 21.6522H129.01C127.897 25.0511 125.568 26.4568 122.337 26.4568C117.846 26.4568 115.055 23.3726 115.055 18.9667C115.055 14.6866 118.014 11.4346 122.337 11.4346C127.184 11.4346 129.555 15.5048 129.261 19.8898H118.979ZM125.358 17.3931C125.002 15.3999 124.141 14.3719 122.253 14.3719C119.714 14.3719 119.084 16.3021 119 17.3931H125.358Z"
                                                                                    fill="#303030" />
                                                                                <path
                                                                                    d="M138.389 27.8415C137.529 30.1284 136.165 31.0726 133.479 31.0726C132.682 31.0726 131.884 31.0096 131.087 30.9257V27.6947C131.821 27.7786 132.598 27.8625 133.374 27.8415C134.402 27.7366 134.885 26.9393 134.885 26.0582C134.885 25.7644 134.822 25.4497 134.717 25.177L129.702 11.8122H133.899L137.13 21.5683H137.193L140.299 11.8122H144.349L138.389 27.8415Z"
                                                                                    fill="#303030" />
                                                                            </svg>
                                                                        </div>
                                                                        <img :src="logoUrl" v-if="hasLogo" alt="Custom logo"
                                                                            style="display: block; max-height: 170px; width: auto"
                                                                            class="em-img" />
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td
                                                                        style="font-size: 24px; line-height: 30px; color: #727272; font-weight: bold; white-space: pre-line;">
                                                                        {{ previewText(emailTemplate.subject) }}
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td style="padding: 40px 0; font-size: 16px; line-height: 20px; white-space: pre-line;"
                                                                        v-html="previewMessage(emailTemplate)">
                                                                    </td>
                                                                </tr>
                                                                <tr v-if="isPasswordSupported(emailTemplate)">
                                                                    <td
                                                                        style="padding: 0px 0 5px; font-size: 16px; line-height: 20px; white-space: pre-line;">
                                                                        {{ previewText(emailTemplate.passwordDescription) }}
                                                                    </td>
                                                                </tr>
                                                                <tr v-if="isPasswordSupported(emailTemplate)">
                                                                    <td
                                                                        style="padding: 0px 0 50px; font-size: 24px; line-height: 30px; color: #727272; font-weight: bold;">
                                                                        43845634
                                                                    </td>
                                                                </tr>
                                                                <tr v-if="isButtonSupported(emailTemplate)">
                                                                    <td>
                                                                        <a href="javascript:void(0);" class="btn-success"
                                                                            style="text-decoration: none; background: #368E19; padding: 10px 12px; text-transform: uppercase; letter-spacing: 0.1em; border-radius: 4px; border: 2px solid #368E19; color: #fff; font-family: 'Trebuchet MS', 'Lucida Sans Unicode', 'Lucida Grande', 'Lucida Sans', Arial, sans-serif; font-size: 14px; box-shadow: none;">
                                                                            {{ previewText(emailTemplate.linkText) }}
                                                                        </a>
                                                                    </td>
                                                                </tr>
                                                            </table>
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td width="100%" align="center">
                                                            <table align="center" width="80%" cellpadding="0"
                                                                cellspacing="0" border="0" style="color: #808080;">
                                                                <!-- <tr>
                                                                    <td align="center" style="padding: 15px 0;">
                                                                        <ul>
                                                                            <li><a href="#" style="color: #808080; text-decoration: none; padding: 0 10px; ">View email in browser</a></li>
                                                                            <li><a href="#" style="color: #808080; text-decoration: none; padding: 0 10px; border-left: 1px solid #808080;">Unsubscribe</a></li>
                                                                            <li><a href="#" style="color: #808080; text-decoration: none; padding: 0 10px; border-left: 1px solid #808080;">Privacy Policy</a></li>
                                                                        </ul>
                                                                    </td>
                                                                </tr> -->
                                                                <tr>
                                                                    <td align="center"
                                                                        style="padding: 15px 0; white-space: pre-line;">
                                                                        Here the address of your organization will appear as
                                                                        specified in your account at the mass mailing
                                                                        server.
                                                                    </td>
                                                                </tr>
                                                                <!-- <tr>
                                                                    <td align="center" style="padding-top: 135px;">Powered by Survey Solutions, <br />
                                                                        IID 34566577886</td>
                                                                </tr> -->
                                                            </table>
                                                        </td>
                                                    </tr>
                                                </table>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </form>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <hr />
        <div class="row mb-05">
            <div class="col-md-12">
                <form v-on:submit.prevent="dummy" :data-vv-scope="'additionalSettings'">
                    <h3>{{ $t('WebInterviewSettings.AdditionalSettings') }}</h3>
                    <div class="form-group mb-20">
                        <input class="checkbox-filter" v-validate="''" data-vv-name="spamProtectionIsEnabled" id="Captcha"
                            type="checkbox" v-model="spamProtectionIsEnabled">
                        <label for="Captcha">
                            <span class="tick"></span>{{ $t('WebInterviewSetup.UseCaptcha') }}
                        </label>
                    </div>
                    <div class="form-group mb-20">
                        <input class="checkbox-filter" v-validate="''" data-vv-name="singleResponse" id="singleResponse"
                            type="checkbox" v-model="singleResponseIsEnabled">
                        <label for="singleResponse">
                            <span class="tick"></span>{{ $t('WebInterviewSetup.SingleResponse') }}
                        </label>
                    </div>
                    <div class="form-group mb-20">
                        <input class="checkbox-filter" v-validate="''" data-vv-name="emailOnComplete" id="emailOnComplete"
                            type="checkbox" v-model="emailOnCompleteIsEnabled">
                        <label for="emailOnComplete">
                            <span class="tick"></span>{{ $t('WebInterviewSetup.EmailOnComplete') }}
                        </label>
                    </div>
                    <div class="form-group mb-20">
                        <input class="checkbox-filter" v-validate="''" data-vv-name="attachAnswersInEmail"
                            id="attachAnswersInEmail" type="checkbox" :disabled="!emailOnCompleteIsEnabled"
                            v-model="attachAnswersInEmailIsEnabled">
                        <label for="attachAnswersInEmail">
                            <span class="tick"></span>{{ $t('WebInterviewSetup.AttachAnswersToCompleteEmail') }}
                        </label>
                    </div>
                    <div class="form-group mb-20">
                        <input class="checkbox-filter" v-validate="''" data-vv-name="allowSwitchToCawiForInterviewer"
                            id="allowSwitchToCawiForInterviewer" type="checkbox"
                            v-model="allowSwitchToCawiForInterviewerEnabled">
                        <label for="allowSwitchToCawiForInterviewer">
                            <span class="tick"></span>{{ $t('WebInterviewSetup.AllowSwitchToCawiForInterviewer') }}
                        </label>
                    </div>
                    <div class="notification-block mb-20">
                        <div class="mb-1">
                            {{ $t('WebInterviewSettings.SendWithNoResponse') }}
                        </div>
                        <select class="selectpicker" v-validate="'required'" tabindex="-98"
                            ref="reminderAfterDaysIfNoResponse" data-vv-name="reminderAfterDaysIfNoResponse"
                            v-model="reminderAfterDaysIfNoResponse">
                            <option value="null">
                                {{ $t('WebInterviewSettings.DoNotSend') }}
                            </option>
                            <option value="1">
                                {{ $t('WebInterviewSettings.AfterXDay', { count: 1 }) }}
                            </option>
                            <option value="2">
                                {{ $t('WebInterviewSettings.AfterXDay', { count: 2 }) }}
                            </option>
                            <option value="3">
                                {{ $t('WebInterviewSettings.AfterXDay', { count: 3 }) }}
                            </option>
                            <option value="5">
                                {{ $t('WebInterviewSettings.AfterXDay', { count: 5 }) }}
                            </option>
                            <option value="7">
                                {{ $t('WebInterviewSettings.AfterXWeek', { count: 1 }) }}
                            </option>
                            <option value="14">
                                {{ $t('WebInterviewSettings.AfterXWeek', { count: 2 }) }}
                            </option>
                        </select>
                    </div>
                    <div class="notification-block mb-30">
                        <div class="mb-1">
                            {{ $t('WebInterviewSettings.SendWithPartialResponse') }}
                        </div>
                        <select class="selectpicker" v-validate="'required'" tabindex="-98"
                            ref="reminderAfterDaysIfPartialResponse" data-vv-name="reminderAfterDaysIfPartialResponse"
                            v-model="reminderAfterDaysIfPartialResponse">
                            <option value="null">
                                {{ $t('WebInterviewSettings.DoNotSend') }}
                            </option>
                            <option value="1">
                                {{ $t('WebInterviewSettings.AfterXDay', { count: 1 }) }}
                            </option>
                            <option value="2">
                                {{ $t('WebInterviewSettings.AfterXDay', { count: 2 }) }}
                            </option>
                            <option value="3">
                                {{ $t('WebInterviewSettings.AfterXDay', { count: 3 }) }}
                            </option>
                            <option value="5">
                                {{ $t('WebInterviewSettings.AfterXDay', { count: 5 }) }}
                            </option>
                            <option value="7">
                                {{ $t('WebInterviewSettings.AfterXWeek', { count: 1 }) }}
                            </option>
                            <option value="14">
                                {{ $t('WebInterviewSettings.AfterXWeek', { count: 2 }) }}
                            </option>
                        </select>
                    </div>
                    <div class="">
                        <button type="submit" :disabled="!isDirty('$additionalSettings')" @click="saveAdditionalSettings()"
                            class="btn btn-md btn-success">
                            {{ $t('WebInterviewSettings.Save') }}
                        </button>
                        <button type="submit" :disabled="!isDirty('$additionalSettings')"
                            @click="cancelAdditionalSettings()" class="btn btn-md btn-link">
                            {{ $t('WebInterviewSettings.Cancel') }}
                        </button>
                    </div>
                </form>
            </div>
        </div>
        <hr />
        <div class="row">
            <div class="col-md-12">
                <div class="">
                    <a href="javascript:void(0);" @click="startWebInterview" v-if="!started"
                        class="btn btn-lg btn-success mb-1" role="button">
                        {{ $t('WebInterviewSetup.Start') }}
                    </a>
                    <a href="javascript:void(0);" @click="stopWebInterview" v-if="started"
                        class="btn btn-lg btn-danger mb-1" role="button">
                        {{ $t('WebInterviewSetup.StopWebInterview') }}
                    </a>
                    <a :href="this.$config.model.surveySetupUrl" class="btn btn-lg back-link mb-1" role="button">
                        {{ $t('WebInterviewSetup.BackToQuestionnaires') }}
                    </a>
                </div>
            </div>
        </div>
    </HqLayout>
</template>

<script>
import Vue from 'vue'
import { marked } from 'marked'
import { map, isNil } from 'lodash'
import { escape } from 'lodash'

import Welcome from './Settings/Welcome'
import Resume from './Settings/Resume'
import Complete from './Settings/Complete'
import Finish from './Settings/Finish'
import LinkInterview from './Settings/Link'

export default {
    components: { Welcome, Resume, Complete, Finish, LinkInterview },

    data() {
        return {
            emailTemplates: [],
            webInterviewPageMessages: [],
            spamProtectionIsEnabled: false,
            singleResponseIsEnabled: true,
            emailOnCompleteIsEnabled: false,
            attachAnswersInEmailIsEnabled: false,
            allowSwitchToCawiForInterviewerEnabled: false,
            started: false,
            reminderAfterDaysIfNoResponse: 3,
            reminderAfterDaysIfPartialResponse: 3,
            logoUrl: '',
            hasLogo: false,
        }
    },

    beforeMount() {

        var self = this
        self.questionnaireId = this.$config.model.questionnaireIdentity.id
        self.questionnaireTitle = this.$config.model.questionnaireTitle
        self.started = this.$config.model.started
        self.spamProtectionIsEnabled = this.$config.model.useCaptcha
        self.singleResponseIsEnabled = this.$config.model.singleResponse
        self.emailOnCompleteIsEnabled = this.$config.model.emailOnComplete
        self.attachAnswersInEmailIsEnabled = this.$config.model.attachAnswersInEmail
        self.allowSwitchToCawiForInterviewerEnabled = this.$config.model.allowSwitchToCawiForInterviewer
        self.reminderAfterDaysIfNoResponse = this.$config.model.reminderAfterDaysIfNoResponse
        self.reminderAfterDaysIfPartialResponse = this.$config.model.reminderAfterDaysIfPartialResponse
        self.cancelSpamProtectionIsEnabled = this.$config.model.useCaptcha
        self.cancelReminderAfterDaysIfNoResponse = this.$config.model.reminderAfterDaysIfNoResponse
        self.cancelReminderAfterDaysIfPartialResponse = this.$config.model.reminderAfterDaysIfPartialResponse
        self.cancelSingleResponseIsEnabled = this.$config.model.singleResponse
        self.cancelEmailOnCompleteIsEnabled = this.$config.model.emailOnComplete
        self.cancelAttachAnswersInEmailIsEnabled = this.$config.model.attachAnswersInEmail
        self.cancelAllowSwitchToCawiForInterviewerEnabled = this.$config.model.allowSwitchToCawiForInterviewer
        self.logoUrl = this.$config.model.logoUrl
        self.hasLogo = this.$config.model.hasLogo

        this.emailTemplates = map(
            this.$config.model.defaultEmailTemplates,
            (value, key) => {
                var defaultEmailTemplate = value
                var custom = self.$config.model.emailTemplates[key]
                var subject = custom == undefined || isNil(custom.subject) || custom.subject === '' ? defaultEmailTemplate.subject : custom.subject
                var message = custom == undefined || isNil(custom.message) || custom.message === '' ? defaultEmailTemplate.message : custom.message
                var passwordDescription = custom == undefined || isNil(custom.passwordDescription) || custom.passwordDescription === '' ? defaultEmailTemplate.passwordDescription : custom.passwordDescription
                var linkText = custom == undefined || isNil(custom.linkText) || custom.linkText === '' ? defaultEmailTemplate.linkText : custom.linkText
                return {
                    value: key,
                    buttonTitle: defaultEmailTemplate.shortTitle,
                    subject: subject,
                    message: message,
                    passwordDescription: passwordDescription,
                    linkText: linkText,
                    isActive: key === 'invitationTemplate',
                }
            }
        )

        map(this.emailTemplates, emailTemplate => {
            self.$validator.reset('emailTemplateData' + emailTemplate.value)
        })

        this.webInterviewPageMessages = map(
            this.$config.model.defaultTexts,
            (value, key) => {
                var customText = self.$config.model.definedTexts[key]
                var defaultText = value
                var message = customText == undefined || isNil(customText) || customText === '' ? defaultText : customText

                message = escape(message)
                return {
                    value: key,
                    text: message,
                    defaultText: defaultText,
                    cancelText: message,
                }
            }
        ).reduce(function (maped, obj) {
            maped[obj.value] = obj
            return maped
        }, {})
    },

    methods: {
        setActive(emailTemplate) {
            map(this.emailTemplates, option => {
                option.isActive = false
            })
            emailTemplate.isActive = true

            var self = this
            this.$nextTick(function () { self.$refs['message' + emailTemplate.value][0].resize() })
        },
        setPageActive(titleType, messageType) {
            this.$nextTick(function () {
                this.$eventHub.$emit('settings:page:active', {
                    titleType, messageType,
                })
            })
        },

        cancelEditEmailTemplate(emailTemplate) {
            var defaultEmailTemplate = this.$config.model.defaultEmailTemplates[emailTemplate.value]
            var custom = this.$config.model.emailTemplates[emailTemplate.value]
            emailTemplate.message = custom == undefined || isNil(custom.message) || custom.message === '' ? defaultEmailTemplate.message : custom.message
            emailTemplate.subject = custom == undefined || isNil(custom.subject) || custom.subject === '' ? defaultEmailTemplate.subject : custom.subject
            emailTemplate.passwordDescription = custom == undefined || isNil(custom.passwordDescription) || custom.passwordDescription === '' ? defaultEmailTemplate.passwordDescription : custom.passwordDescription
            emailTemplate.linkText = custom == undefined || isNil(custom.linkText) || custom.linkText === '' ? defaultEmailTemplate.linkText : custom.linkText
            this.$validator.reset('emailTemplateData' + emailTemplate.value)
        },

        async saveEmailTemplate(emailTemplate) {
            var self = this
            var validationResult = await this.$validator.validateAll('emailTemplateData' + emailTemplate.value)
            if (validationResult) {
                self.$store.dispatch('showProgress')
                await this.$hq.WebInterviewSettings.updateEmailTemplate(this.questionnaireId, emailTemplate.value, emailTemplate.subject, emailTemplate.message, emailTemplate.passwordDescription, emailTemplate.linkText)
                    .then(function (response) {
                        if (!self.$config.model.emailTemplates[emailTemplate.value]) {
                            self.$config.model.emailTemplates[emailTemplate.value] = []
                        }
                        var userTemplate = self.$config.model.emailTemplates[emailTemplate.value]
                        userTemplate.subject = emailTemplate.subject
                        userTemplate.message = emailTemplate.message
                        userTemplate.passwordDescription = emailTemplate.passwordDescription
                        userTemplate.linkText = emailTemplate.linkText

                        self.$validator.reset('emailTemplateData' + emailTemplate.value)
                    })
                    .catch(function (error) {
                        Vue.config.errorHandler(error, self)
                    })
                    .then(function () {
                        self.$store.dispatch('hideProgress')
                    })
            }
        },
        webInterviewPageText(type) {
            return this.webInterviewPageMessages[type].text
        },
        enablePageTextEditMode(type) {
            this.webInterviewPageMessages[type].isEditMode = true
        },
        isEditModePageTextEditMode(type) {
            return this.webInterviewPageMessages[type].isEditMode
        },
        async startWebInterview() {
            var self = this
            self.$store.dispatch('showProgress')
            await this.$hq.WebInterviewSettings.startWebInterview(this.questionnaireId)
                .then(function (response) {
                    self.started = true
                })
                .catch(function (error) {
                    Vue.config.errorHandler(error, self)
                })
                .then(function () {
                    self.$store.dispatch('hideProgress')
                })
        },
        async stopWebInterview() {
            var self = this
            self.$store.dispatch('showProgress')
            await this.$hq.WebInterviewSettings.stopWebInterview(this.questionnaireId)
                .then(function (response) {
                    self.started = false
                })
                .catch(function (error) {
                    Vue.config.errorHandler(error, self)
                })
                .then(function () {
                    self.$store.dispatch('hideProgress')
                })
        },
        async saveAdditionalSettings() {
            var self = this
            self.$store.dispatch('showProgress')
            await this.$hq.WebInterviewSettings.updateAdditionalSettings(this.questionnaireId,
                this.spamProtectionIsEnabled,
                this.reminderAfterDaysIfNoResponse == 'null' ? null : this.reminderAfterDaysIfNoResponse,
                this.reminderAfterDaysIfPartialResponse == 'null' ? null : this.reminderAfterDaysIfPartialResponse,
                this.singleResponseIsEnabled,
                this.emailOnCompleteIsEnabled,
                this.attachAnswersInEmailIsEnabled,
                this.allowSwitchToCawiForInterviewerEnabled)
                .then(function (response) {
                    self.cancelSpamProtectionIsEnabled = self.spamProtectionIsEnabled
                    self.cancelReminderAfterDaysIfNoResponse = self.reminderAfterDaysIfNoResponse
                    self.cancelReminderAfterDaysIfPartialResponse = self.reminderAfterDaysIfPartialResponse
                    self.cancelSingleResponseIsEnabled = self.singleResponseIsEnabled
                    self.cancelEmailOnCompleteIsEnabled = self.emailOnCompleteIsEnabled
                    self.cancelAttachAnswersInEmailIsEnabled = self.attachAnswersInEmailIsEnabled
                    self.cancelAllowSwitchToCawiForInterviewerEnabled = self.allowSwitchToCawiForInterviewerEnabled
                    self.$validator.reset('additionalSettings')
                })
                .catch(function (error) {
                    Vue.config.errorHandler(error, self)
                })
                .then(function () {
                    self.$store.dispatch('hideProgress')
                })
        },
        cancelAdditionalSettings() {
            this.spamProtectionIsEnabled = this.cancelSpamProtectionIsEnabled
            this.reminderAfterDaysIfNoResponse = this.cancelReminderAfterDaysIfNoResponse
            this.reminderAfterDaysIfPartialResponse = this.cancelReminderAfterDaysIfPartialResponse
            this.singleResponseIsEnabled = this.cancelSingleResponseIsEnabled
            this.emailOnCompleteIsEnabled = this.cancelEmailOnCompleteIsEnabled
            this.attachAnswersInEmailIsEnabled = this.cancelAttachAnswersInEmailIsEnabled
            this.allowSwitchToCawiForInterviewerEnabled = this.cancelAllowSwitchToCawiForInterviewerEnabled
            this.$validator.reset('additionalSettings')
        },
        previewHtml(text) {
            var html = marked(this.previewText(text))
            return html
        },
        previewText(text) {
            if (text == null)
                return ''

            return text
                .replace(/%SURVEYNAME%/g, this.questionnaireTitle)
                .replace(/%QUESTIONNAIRE%/g, this.questionnaireTitle)
        },
        previewMessage(emailTemplate) {
            var text = this.previewText(emailTemplate.message)
            text = escape(text)

            if (!this.isMessageSupportedInterviewData(emailTemplate))
                return text

            return text
                .replace(/%[A-Za-z0-9_]+%/g, match => this.$t('WebInterviewSettings.AnswerOn', { variable: match.replace(/%/g, '') }))
                .replace(/%[A-Za-z0-9_]+:barcode%/g, '<img src="/img/barcode128.png" />')
                .replace(/%[A-Za-z0-9_]+:qrcode%/g, '<img src="/img/qrcode.png" />')
        },
        dummy() {
            return false
        },
        isDirty(formName) {
            const form = this.fields[formName]
            const keys = Object.keys((this.fields || {})[formName] || {})
            return keys.some(key => form[key].dirty || form[key].changed)
        },
        async clearField(emailTemplate, fieldName) {
            emailTemplate[fieldName] = null
            await this.$nextTick()
            await this.$validator.validate('emailTemplateData' + emailTemplate.value + '.' + fieldName)
        },
        isPasswordSupported(emailTemplate) {
            return emailTemplate.value != 'completeInterviewEmail'
        },
        isButtonSupported(emailTemplate) {
            return emailTemplate.value != 'completeInterviewEmail'
        },
        isMessageSupportedInterviewData(emailTemplate) {
            return emailTemplate.value == 'completeInterviewEmail'
        },
    },
    watch: {
        reminderAfterDaysIfNoResponse: function (val) {
            if (this.$refs.reminderAfterDaysIfNoResponse.value != val) {
                this.$refs.reminderAfterDaysIfNoResponse.value = val
                $(this.$refs.reminderAfterDaysIfNoResponse).selectpicker('refresh')
            }
        },
        reminderAfterDaysIfPartialResponse: function (val) {
            if (this.$refs.reminderAfterDaysIfPartialResponse.value != val) {
                this.$refs.reminderAfterDaysIfPartialResponse.value = val
                $(this.$refs.reminderAfterDaysIfPartialResponse).selectpicker('refresh')
            }
        },
    },
    computed: {

    },
}
</script>

