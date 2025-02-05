<template>
    <div class="container-wb-list">
        <div class="header-wb-list">
            <header>

                <div class="questionnaire-logo">
                    <a class="logo" href="https://mysurvey.solutions/" target="_blank" rel="noopener">
                        <img src="../../../content/i/logo.png" alt="Survey Solutions" class="pull-left">
                        <span class="pull-left">Survey<br>Solutions</span>
                    </a>

                    <a class="navbar-text" href="/">{{ $t('QuestionnaireController.Designer') }}</a>
                </div>

                <ul class="nav nav-tabs">
                    <li role="presentation">
                        <a href="/questionnaire/my">{{ $t('QuestionnaireController.MyQuestionnaires') }}</a>
                    </li>
                    <li role="presentation">
                        <a href="/questionnaire/shared">{{ $t('QuestionnaireController.SharedQuestionnaires') }}</a>
                    </li>
                    <li role="presentation">
                        <a href="/questionnaire/public">{{ $t('QuestionnaireController.PublicQuestionnaires') }}</a>
                    </li>
                    <li role="presentation" :class="{ active: activePage == 'classifications' }">
                        <a href="/classifications">{{ $t('QuestionnaireController.Classifications') }}</a>
                    </li>
                    <li v-if="isAdmin" role="presentation">
                        <a href="/Admin/Users">{{ $t('QuestionnaireController.ManageUsers') }}</a>
                    </li>
                    <li class="create" role="presentation"><a href="/questionnaire/create">{{
                        $t('QuestionnaireController.CreateNew') }}</a></li>
                </ul>
                <nav>
                    <a class="btn btn-default" target="_blank" href="http://support.mysurvey.solutions/designer"
                        rel="noopener">{{
                        $t('AccountResources.Help') }}</a>
                    <a class="btn btn-default" href="https://forum.mysurvey.solutions" target="_blank" rel="noopener">{{
                        $t('AccountResources.Forum') }}</a>
                    <a v-if="isAdmin" class="btn btn-default" href="/admin/controlpanel">
                        {{ $t('QuestionnaireController.ControlPanel') }}
                    </a>
                    <div class="btn-group">
                        <a class="btn btn-default">
                            {{ $t('QuestionnaireEditor.HellowMessageBtn', {
                        currentUserName: userName
                    }) }}</a>
                        <a class="btn btn-default dropdown-toggle" data-toggle="dropdown" aria-haspopup="true"
                            aria-expanded="false">
                            <span class="caret"></span>
                            <span class="sr-only"></span>
                        </a>
                        <ul class="dropdown-menu dropdown-menu-right">
                            <li>
                                <a href="/identity/account/manage">{{
                        $t('AccountResources.ManageAccount')
                    }}</a>
                            </li>
                            <li>
                                <a href="/identity/account/manage/changepassword">{{
                            $t('AccountResources.ChangePassword')
                        }}</a>
                            </li>
                            <li>
                                <a href="/identity/account/logout">{{
                                    $t('AccountResources.LogOut')
                                    }}</a>
                            </li>
                        </ul>
                    </div>

                </nav>
            </header>
        </div>
        <section id="designer-list">
            <router-view></router-view>
        </section>
    </div>
</template>


<script>

import '../../../content/designer-start/bootstrap-custom.less';
import '../../../content/designer-start/designer-list.less'

export default {
    name: 'DesignerLayout',

    props: {
        activePage: { type: String, required: true },
    },

    data() {
        return {

        };
    },

    computed: {
        userName() {
            return this.$store.state.userName;
        },
        isAdmin() {
            return this.$store.state.isAdmin;
        },
    },

    mounted() {
        /*if (window.moment) {
            moment.locale('@CultureInfo.CurrentUICulture');

            $('time').each(function () {
                var me = $(this);
                var date = me.attr('datetime') || me.attr('time');
                var momentDate = moment.utc(date).local().format('MMM DD, YYYY HH:mm');
                me.text(momentDate);
            });
        }*/
    },

    methods: {

    }
};
</script>
