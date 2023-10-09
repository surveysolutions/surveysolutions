<template>
    <v-app>
        <v-main>
            <div id="designer-editor" class="questionnaire container">
                <router-view name="header"></router-view>
                <section id="spacer" class="row">
                    <router-view />
                </section>
            </div>
        </v-main>
    </v-app>
</template>

<script>
import { provide } from 'vue';
//import store from '@/store'
import { useQuestionnaireStore } from './stores/questionnaire';

export default {
    name: 'QuestionnaireApp',
    provide: { useQuestionnaireStore }
};
</script>
