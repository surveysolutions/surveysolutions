<template>
    <!--v-app>
        <v-main-->
    <div id="designer-editor" class="questionnaire container">
        <router-view name="header"></router-view>
        <section id="spacer" class="row">
            <div class="left"></div>
            <div class="right"></div>
        </section>
        <section id="main" class="row">
            <router-view name="leftSidePanel"></router-view>
            <router-view />
        </section>
    </div>
    <confirm-dialog></confirm-dialog>
    <!--/v-main>
    </v-app-->
</template>

<script>
import { provide } from 'vue';
import ConfirmDialog from './views/App/components/Confirm.vue';
//import store from '@/store'
import { useQuestionnaireStore } from './stores/questionnaire';

export default {
    name: 'QuestionnaireApp',
    provide: { useQuestionnaireStore },
    components: {
        ConfirmDialog
    }
};
</script>
